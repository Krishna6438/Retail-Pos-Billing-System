namespace BillingService.DTOs.Invoice;

public class InvoiceItemDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
