namespace BillingService.Models;

public class Return
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}