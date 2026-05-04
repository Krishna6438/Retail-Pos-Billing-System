namespace ProductService.DTOs.Tax;

public class CreateTaxDTO
{
    public string Name { get; set; } = string.Empty;
    
    public decimal TaxPercentage { get; set; }
}