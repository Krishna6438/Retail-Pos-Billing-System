namespace BillingService.DTOs;

public class BillResponseDTO
{
    public int BillId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}