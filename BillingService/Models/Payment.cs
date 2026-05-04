namespace BillingService.Models;

public class Payment
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public string Method { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public Bill? Bill { get; set; }
    
    public string Status { get; set; } = "Pending";

    public string? TransactionReference { get; set; }
}