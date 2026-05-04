namespace AdminService.DTOs;

public class DashboardDTO
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public List<object>? TopProducts { get; set; }
    public List<object>? LowStockProducts { get; set; }
}