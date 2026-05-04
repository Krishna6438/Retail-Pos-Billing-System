namespace BillingService.Models;

public class Shift
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public DateTime OpenedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
    
    public decimal StartingFloat { get; set; }
    
    public decimal ExpectedCash { get; set; }
    
    public decimal? ActualCash { get; set; }
    
    public decimal? Variance { get; set; }
    
    public string Status { get; set; } = "Open"; // Open, Closed
}
