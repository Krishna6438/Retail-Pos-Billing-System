namespace BillingService.Models;

public class BillItem
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TaxAmount { get; set; }

    public Bill? Bill { get; set; }
}
