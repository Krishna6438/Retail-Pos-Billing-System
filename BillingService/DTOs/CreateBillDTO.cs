namespace BillingService.DTOs;

public class CreateBillDTO
{
    public int UserId { get; set; }

    public List<BillItemDTO> Items { get; set; } = new();

    public string PaymentMethod { get; set; } = string.Empty;
}
