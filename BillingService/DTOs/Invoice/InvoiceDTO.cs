namespace BillingService.DTOs.Invoice;

public class InvoiceDTO
{
    public int BillId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string StoreName { get; set; } = "RetailPOS Urban Market";
    public string StoreAddress { get; set; } = "MG Road, Bengaluru, Karnataka 560001";
    public string CurrencyCode { get; set; } = "INR";
    public string CashierName { get; set; } = "Counter Team";
    public decimal TotalAmount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? CouponCode { get; set; }

    public List<InvoiceItemDTO>? Items { get; set; }
}
