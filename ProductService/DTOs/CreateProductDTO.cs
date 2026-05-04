public class CreateProductDTO
{
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }

    public int InitialQuantity { get; set; }
}
