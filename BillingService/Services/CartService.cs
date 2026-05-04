using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BillingService.Services;

public class CartService
{
    private readonly AppDbContext _context;
    private readonly ProductClient _productClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RabbitMQPublisher _publisher;

    public CartService(
        AppDbContext context,
        ProductClient productClient,
        IHttpContextAccessor httpContextAccessor,
        RabbitMQPublisher publisher)
    {
        _context = context;
        _productClient = productClient;
        _httpContextAccessor = httpContextAccessor;
        _publisher = publisher;
    }

    public bool CanAccessCart(int cartId, int userId)
    {
        return _context.Carts.Any(c => c.Id == cartId && c.UserId == userId);
    }

    private string GetTokenOrThrow()
    {
        var token = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"]
            .ToString();

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("Authorization token missing");

        return token;
    }

    private int GetCurrentUserIdOrThrow()
    {
        var claimValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claimValue, out var userId))
            throw new Exception("Invalid user context");

        return userId;
    }

    public async Task<string> AddItem(AddToCartDTO dto)
    {
        var token = GetTokenOrThrow();
        var currentUserId = GetCurrentUserIdOrThrow();

        if (dto.UserId != currentUserId)
            throw new Exception("You can only modify your own cart");

        //  Get or create cart
        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == dto.UserId && !c.IsHeld);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = dto.UserId,
                Items = new List<CartItem>()
            };

            _context.Carts.Add(cart);
        }

        //  Get product from ProductService
        var product = await _productClient.GetProduct(dto.ProductId, token);

        if (product == null)
            throw new Exception("Product not found");

        if (dto.Quantity > product.Quantity)
            throw new Exception("Insufficient stock");

        //  Check existing item
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        var requestedQuantity = dto.Quantity;

        if (existingItem != null)
        {
            requestedQuantity += existingItem.Quantity;
        }

        if (requestedQuantity > product.Quantity)
        {
            throw new Exception("Requested quantity exceeds available stock");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = requestedQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Price = product.Price
            });
        }

        await _context.SaveChangesAsync();

        return "Item added to cart";
    }

    //  GET CART (with validation)
    public async Task<CartResponseDTO?> GetCart(int userId)
    {
        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == userId && !c.IsHeld);

        if (cart == null)
            return null;

        var token = GetTokenOrThrow();
        decimal subtotal = 0;
        decimal taxAmount = 0;

        foreach (var item in cart.Items)
        {
            var product = await _productClient.GetProduct(item.ProductId, token);
            var unitPrice = product?.Price ?? item.Price;
            var lineSubtotal = unitPrice * item.Quantity;
            var lineTaxAmount = lineSubtotal * ((product?.TaxPercentage ?? 0) / 100m);

            subtotal += lineSubtotal;
            taxAmount += lineTaxAmount;
        }

        var total = subtotal + taxAmount;

        return new CartResponseDTO
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDTO
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList(),
            SubtotalAmount = subtotal,
            TaxAmount = taxAmount,
            TotalAmount = total
        };
    }
    
    public async Task<string> UpdateItem(UpdateCartDTO dto)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (dto.UserId != currentUserId)
            throw new Exception("You can only modify your own cart");

        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == dto.UserId && !c.IsHeld);

        if (cart == null)
            throw new Exception("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

        if (item == null)
            throw new Exception("Item not found in cart");

        if (dto.Quantity <= 0)
            throw new Exception("Quantity must be greater than 0");

        var token = GetTokenOrThrow();
        var product = await _productClient.GetProduct(dto.ProductId, token);

        if (product == null)
            throw new Exception("Product not found");

        if (dto.Quantity > product.Quantity)
            throw new Exception("Requested quantity exceeds available stock");

        item.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        return "Cart updated successfully";
    }
    
    public async Task<string> RemoveItem(int userId, int productId)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (userId != currentUserId)
            throw new Exception("You can only modify your own cart");

        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.UserId == userId && !c.IsHeld);

        if (cart == null)
            throw new Exception("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new Exception("Item not found in cart");

        cart.Items.Remove(item);

        await _context.SaveChangesAsync();

        return "Item removed successfully";
    }
    
    public async Task<string> HoldCart(int cartId)
    {
        var cart = _context.Carts.FirstOrDefault(c => c.Id == cartId);

        if (cart == null)
            throw new Exception("Cart not found");

        if (cart.IsHeld)
            throw new Exception("Cart already held");

        cart.IsHeld = true;

        await _context.SaveChangesAsync();

        return "Cart held successfully";
    }
    
    public async Task<string> ResumeCart(int cartId)
    {
        var cart = _context.Carts.FirstOrDefault(c => c.Id == cartId);

        if (cart == null)
            throw new Exception("Cart not found");

        if (!cart.IsHeld)
            throw new Exception("Cart is not in held state");

        cart.IsHeld = false;

        await _context.SaveChangesAsync();

        return "Cart resumed successfully";
    }
    
    public async Task<BillResponseDTO> Checkout(int cartId, CheckoutDTO dto)
    {
        var token = GetTokenOrThrow();

        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.Id == cartId && !c.IsHeld);

        if (cart == null)
            throw new Exception("Cart not found or is held");

        var currentShift = _context.Shifts.FirstOrDefault(s => s.UserId == cart.UserId && s.Status == "Open");
        if (currentShift == null)
            throw new Exception("You must open a shift before checking out.");

        decimal total = 0;
        var billItems = new List<BillItem>();

        foreach (var item in cart.Items)
        {
            var product = await _productClient.GetProduct(item.ProductId, token);

            if (product == null)
                throw new Exception("Product not found");

            if (product.Quantity < item.Quantity)
                throw new Exception("Insufficient stock");

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

        if (!string.IsNullOrWhiteSpace(dto.CouponCode))
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == dto.CouponCode);

            if (coupon == null || coupon.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Invalid or expired coupon");

            total -= total * (coupon.DiscountPercentage / 100m);
        }

        var bill = new Bill
        {
            UserId = cart.UserId,
            ShiftId = currentShift.Id,
            TotalAmount = total,
            Items = billItems,
            Payment = new Payment
            {
                Method = dto.PaymentMethod ?? "Cash",
                Amount = total,
                Status = "Completed",
                TransactionReference = dto.UpiId ?? dto.CardNumber
            }
        };

        _context.Bills.Add(bill);

        //  Clear cart
        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync();

        _publisher.PublishBillCreated(new Events.BillCreatedEvent
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
    
    public async Task<string> ClearCart(int cartId)
    {
        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.Id == cartId);

        if (cart == null)
            throw new Exception("Cart not found");

        if (cart.Items == null || !cart.Items.Any())
            return "Cart already empty";

        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();

        return "Cart cleared successfully";
    }
}
