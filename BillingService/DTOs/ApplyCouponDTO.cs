using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class ApplyCouponDTO
{
    [Range(1, int.MaxValue)]
    public int BillId { get; set; }
    [Required]
    public string Code { get; set; } = string.Empty;
}
