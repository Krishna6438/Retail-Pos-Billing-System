namespace ProductService.DTOs;

public class ProductResponseDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TaxPercentage { get; set; }
}
