namespace BillingService.DTOs;

public class CartResponseDTO
{
    public int CartId { get; set; }
    public int UserId { get; set; }

    public List<CartItemDTO> Items { get; set; } = new();

    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
