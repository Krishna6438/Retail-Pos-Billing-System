using BillingService.DTOs;
using BillingService.DTOs.Analytics;
using BillingService.DTOs.Invoice;
using BillingService.Events;
using BillingService.Models;
using BillingService.Repositories;
using Microsoft.AspNetCore.Http;

namespace BillingService.Services;

public class BillingService
{
    private readonly BillingRepository _repo;
    private readonly ProductClient _productClient;
    private readonly PdfService _pdfService;
    private readonly RabbitMQPublisher _publisher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BillingService(
        BillingRepository repo,
        ProductClient productClient,
        PdfService pdfService,
        RabbitMQPublisher publisher,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _productClient = productClient;
        _pdfService = pdfService;
        _publisher = publisher;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool CanAccessBill(int billId, int userId)
    {
        var ownerId = _repo.GetBillOwnerId(billId);
        return ownerId.HasValue && ownerId.Value == userId;
    }

    public int? GetCartOwnerId(int cartId)
        => _repo.GetCartOwnerId(cartId);

    
    //  CREATE BILL
    
    public async Task<BillResponseDTO> CreateBill(CreateBillDTO dto, string token)
    {
        if (dto.Items.Count == 0)
            throw new Exception("Bill must contain at least one item");

        decimal total = 0;
        var billItems = new List<BillItem>();
        var products = new List<(ProductResponse product, int quantity)>();

        //  VALIDATE PRODUCTS
        foreach (var item in dto.Items)
        {
            var product = await _productClient.GetProduct(item.ProductId, token);

            if (product == null)
                throw new Exception($"Product {item.ProductId} not found");

            if (product.Quantity < item.Quantity)
                throw new Exception($"Insufficient stock for product {item.ProductId}");

            products.Add((product, item.Quantity));
        }

        // CALCULATE TOTAL
        foreach (var (product, quantity) in products)
        {
            var taxAmount = product.Price * quantity * (product.TaxPercentage / 100);

            total += (product.Price * quantity) + taxAmount;

            billItems.Add(new BillItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                Price = product.Price,
                TaxAmount = taxAmount
            });
        }

        foreach (var (product, quantity) in products)
        {
            var updated = await _productClient.UpdateInventory(product.Id, quantity, token);

            if (!updated)
                throw new Exception($"Inventory update failed for product {product.Id}");
        }

        // SAVE BILL
        var bill = new Bill
        {
            UserId = dto.UserId,
            TotalAmount = total,
            Items = billItems,
            Payment = new Payment
            {
                Method = dto.PaymentMethod,
                Amount = total,
                Status = "Completed"
            }
        };

        _repo.AddBill(bill);

        //  BILL CREATED EVENT
        _publisher.PublishBillCreated(new BillCreatedEvent
        {
            BillId = bill.Id,
            UserId = bill.UserId,
            TotalAmount = bill.TotalAmount
        });

        return new BillResponseDTO
        {
            BillId = bill.Id,
            TotalAmount = bill.TotalAmount,
            CreatedAt = bill.CreatedAt
        };
    }

    
    //  GET BILL
   
    public BillResponseDTO? GetBill(int billId)
        => _repo.GetBill(billId);

    
    //  ANALYTICS
    
    public DailySalesDTO GetTodaySales()
        => _repo.GetTodaySales();

    public decimal GetTotalRevenue()
        => _repo.GetTotalRevenue();

    public async Task<List<TopProductDTO>> GetTopProducts()
    {
        var top = _repo.GetTopProducts();
        var token = GetTokenOrEmpty();

        foreach (var item in top)
        {
            var product = string.IsNullOrWhiteSpace(token)
                ? null
                : await _productClient.GetProduct(item.ProductId, token);

            item.ProductName = product?.Name ?? $"Product #{item.ProductId}";
        }

        return top;
    }

    
    //  FILTERS
    
    public List<Bill> GetBillsByUser(int userId)
        => _repo.GetBillsByUser(userId);

    public List<Bill> GetBillsByDate(DateTime date)
        => _repo.GetBillsByDate(date);

    
    //  COUPONS
    
    public string CreateCoupon(CreateCouponDTO dto)
    {
        if (_repo.GetCoupon(dto.Code) != null)
            throw new Exception("Coupon already exists");

        var coupon = new Coupon
        {
            Code = dto.Code,
            DiscountPercentage = dto.DiscountPercentage,
            ExpiryDate = dto.ExpiryDate
        };

        _repo.AddCoupon(coupon);
        return "Coupon created";
    }

    public Coupon? GetCouponDetails(string code)
    {
        var coupon = _repo.GetCoupon(code);
        if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow) return null;
        return coupon;
    }

    public string ApplyCoupon(ApplyCouponDTO dto)
    {
        var coupon = _repo.GetCoupon(dto.Code);

        if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow)
            throw new Exception("Invalid or expired coupon");

        var bill = _repo.GetBillById(dto.BillId);

        if (bill == null)
            throw new Exception("Bill not found");

        var discount = bill.TotalAmount * (coupon.DiscountPercentage / 100);
        bill.TotalAmount -= discount;

        _repo.UpdateBill(bill);

        return $"Coupon applied. Discount: {discount}";
    }

   
    //  PAYMENT
   
    public string UpdatePaymentStatus(int billId, string status)
    {
        var bill = _repo.GetBillById(billId);

        if (bill == null || bill.Payment == null)
            throw new Exception("Bill not found");

        bill.Payment.Status = status;

        _repo.UpdateBill(bill);

        //  PAYMENT EVENT
        _publisher.PublishPaymentCompleted(new PaymentCompletedEvent
        {
            BillId = billId,
            UserId = bill.UserId,
            Status = status
        });

        return "Payment status updated";
    }

    
    //  REFUND
   
    public string Refund(int billId)
    {
        var bill = _repo.GetBillById(billId);

        if (bill == null || bill.Payment == null)
            throw new Exception("Bill not found");

        bill.Payment.Status = "Refunded";

        _repo.UpdateBill(bill);

        _publisher.PublishPaymentCompleted(new PaymentCompletedEvent
        {
            BillId = billId,
            UserId = bill.UserId,
            Status = "Refunded"
        });

        return "Refund processed";
    }

    
    // CHECKOUT FROM CART
    public async Task<BillResponseDTO> CheckoutFromCart(int cartId, string token)
    {
        var cart = _repo.GetCartById(cartId);

        if (cart == null || !cart.Items.Any())
            throw new Exception("Cart is empty");

        decimal total = 0;
        var billItems = new List<BillItem>();

        foreach (var item in cart.Items)
        {
            var product = await _productClient.GetProduct(item.ProductId, token);

            if (product == null)
                throw new Exception($"Product {item.ProductId} not found");

            if (product.Quantity < item.Quantity)
                throw new Exception($"Insufficient stock for product {item.ProductId}");

            var taxAmount = product.Price * item.Quantity * (product.TaxPercentage / 100);

            total += (product.Price * item.Quantity) + taxAmount;

            billItems.Add(new BillItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price,
                TaxAmount = taxAmount
            });
        }

        foreach (var item in cart.Items)
        {
            var updated = await _productClient.UpdateInventory(item.ProductId, item.Quantity, token);

            if (!updated)
                throw new Exception($"Inventory update failed for product {item.ProductId}");
        }

        var bill = new Bill
        {
            UserId = cart.UserId,
            TotalAmount = total,
            Items = billItems,
            Payment = new Payment
            {
                Method = "Cash",
                Amount = total,
                Status = "Completed"
            }
        };

        _repo.AddBill(bill);
        _repo.DeleteCart(cartId);

        _publisher.PublishBillCreated(new BillCreatedEvent
        {
            BillId = bill.Id,
            UserId = bill.UserId,
            TotalAmount = bill.TotalAmount
        });

        return new BillResponseDTO
        {
            BillId = bill.Id,
            TotalAmount = bill.TotalAmount,
            CreatedAt = bill.CreatedAt
        };
    }

   
    //  INVOICE
    public async Task<List<InvoiceDTO>> GetOrderHistory(int userId)
    {
        var bills = _repo.GetBillsWithItemsByUser(userId);
        var token = GetTokenOrEmpty();
        var invoices = new List<InvoiceDTO>();

        foreach (var bill in bills.OrderByDescending(b => b.CreatedAt))
        {
            invoices.Add(await BuildInvoiceAsync(bill, token));
        }

        return invoices;
    }

    public async Task<InvoiceDTO?> GetInvoice(int billId)
    {
        var bill = _repo.GetInvoice(billId);

        if (bill == null)
            return null;

        return await BuildInvoiceAsync(bill, GetTokenOrEmpty());
    }

    public async Task<byte[]> GenerateInvoicePdf(int billId)
    {
        var invoice = await GetInvoice(billId);

        if (invoice == null)
            throw new Exception("Invoice not found");

        return _pdfService.GenerateInvoicePdf(invoice);
    }

    private string GetTokenOrEmpty()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString() ?? string.Empty;
    }

    private async Task<InvoiceDTO> BuildInvoiceAsync(Bill bill, string token)
    {
        var items = new List<InvoiceItemDTO>();

        foreach (var item in bill.Items.OrderBy(i => i.ProductId))
        {
            var product = string.IsNullOrWhiteSpace(token)
                ? null
                : await _productClient.GetProduct(item.ProductId, token);

            items.Add(new InvoiceItemDTO
            {
                ProductId = item.ProductId,
                ProductName = product?.Name ?? $"Product #{item.ProductId}",
                Barcode = product?.Barcode ?? "NA",
                Quantity = item.Quantity,
                Price = item.Price,
                TaxAmount = item.TaxAmount,
                LineTotal = (item.Price * item.Quantity) + item.TaxAmount
            });
        }

        var subtotalAmount = items.Sum(item => item.Price * item.Quantity);
        var taxAmount = items.Sum(item => item.TaxAmount);
        var expectedTotal = subtotalAmount + taxAmount;
        var discountAmount = Math.Max(expectedTotal - bill.TotalAmount, 0);

        return new InvoiceDTO
        {
            BillId = bill.Id,
            InvoiceNumber = $"INV-{bill.CreatedAt:yyyyMMdd}-{bill.Id:D5}",
            UserId = bill.UserId,
            TotalAmount = bill.TotalAmount,
            SubtotalAmount = subtotalAmount,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            CreatedAt = bill.CreatedAt,
            PaymentMethod = bill.Payment?.Method,
            PaymentStatus = bill.Payment?.Status,
            CouponCode = discountAmount > 0 ? "Applied at checkout" : null,
            Items = items
        };
    }
}
