namespace BillingService.DTOs.Analytics;

public class DailySalesDTO
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
}