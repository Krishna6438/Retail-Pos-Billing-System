namespace ProductService.DTOs.Tax;

public class UpdateTaxDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }
}