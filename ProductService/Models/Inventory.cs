namespace ProductService.Models;

public class Inventory
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}
