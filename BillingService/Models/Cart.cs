namespace BillingService.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public bool IsHeld { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}
