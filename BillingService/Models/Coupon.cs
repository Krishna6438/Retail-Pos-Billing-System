namespace BillingService.Models;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
}