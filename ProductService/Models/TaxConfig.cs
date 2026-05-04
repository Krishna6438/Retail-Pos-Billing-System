namespace ProductService.Models;

public class TaxConfig
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public decimal TaxPercentage { get; set; }

    // Navigation
    public ICollection<Product>? Products { get; set; }
}