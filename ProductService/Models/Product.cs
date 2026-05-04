namespace ProductService.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int TaxConfigId { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
    public TaxConfig TaxConfig { get; set; } = null!;
    public Inventory Inventory { get; set; } = null!;
}
