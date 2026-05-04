namespace BillingService.Models;

public class Bill
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public int UserId { get; set; }

    public int? ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    public Payment? Payment { get; set; }
}
