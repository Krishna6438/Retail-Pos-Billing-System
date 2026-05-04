using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class CreateCouponDTO
{
    [Required]
    public string Code { get; set; } = string.Empty;
    [Range(0.01, 100)]
    public decimal DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }
}
