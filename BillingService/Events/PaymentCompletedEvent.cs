namespace BillingService.Events;

public class PaymentCompletedEvent
{
    public int BillId { get; set; }
    public int UserId { get; set; }
    public string? Status { get; set; }
}
