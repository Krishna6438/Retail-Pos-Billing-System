using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class CheckoutDTO
{
    [Required]
    public string? PaymentMethod { get; set; } // Cash, UPI, Card

    public string? CouponCode { get; set; }

    public string? UpiId { get; set; }

    public string? CardNumber { get; set; }
}
