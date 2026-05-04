namespace NotificationService.Events;

public class BillCreatedEvent
{
    public int BillId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
}
