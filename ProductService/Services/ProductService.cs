using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class ProductService
{
    private readonly ProductRepository _repo;
    private static readonly HashSet<int> SupportedBarcodeLengths = [8, 12, 13];

    public ProductService(ProductRepository repo)
    {
        _repo = repo;
    }

    public string AddProduct(CreateProductDTO dto)
    {
        var barcode = string.IsNullOrWhiteSpace(dto.Barcode)
            ? GenerateBarcode()
            : NormalizeBarcode(dto.Barcode);

        if (!IsValidBarcode(barcode))
            throw new Exception("Barcode must contain 8, 12, or 13 digits");

        if (_repo.ExistsByBarcode(barcode))
            throw new Exception("A product already exists with this barcode");

        var category = _repo.GetCategoryByName(dto.CategoryName);

        if (category == null)
        {
            category = new Category { Name = dto.CategoryName };
            _repo.AddCategory(category);
        }

        var tax = _repo.GetTaxByPercentage(dto.TaxPercentage);

        if (tax == null)
        {
            tax = new TaxConfig { TaxPercentage = dto.TaxPercentage };
            _repo.AddTax(tax);
        }

        var product = new Product
        {
            Name = dto.Name,
            Barcode = barcode,
            Price = dto.Price,
            CategoryId = category.Id,
            TaxConfigId = tax.Id
        };

        var inventory = new Inventory
        {
            Quantity = dto.InitialQuantity,
            Product = product
        };

        product.Inventory = inventory;

        _repo.Add(product);

        return "Product added successfully";
    }
    
    public List<ProductResponseDTO> GetAll()
    {
        var products = _repo.GetAll();

        return products.Select(p => new ProductResponseDTO
        {
            Id = p.Id,
            Name = p.Name,
            Barcode = p.Barcode,
            Price = p.Price,
            CategoryName = p.Category.Name,
            Quantity = p.Inventory.Quantity,
            TaxPercentage = p.TaxConfig.TaxPercentage
        }).ToList();
    }


    public ProductResponseDTO? GetByBarcode(string barcode)
    {
        var product = _repo.GetByBarcode(NormalizeBarcode(barcode));

        if (product == null)
            return null;

        return new ProductResponseDTO
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode,
            Price = product.Price,
            CategoryName = product.Category.Name,
            Quantity = product.Inventory.Quantity,
            TaxPercentage = product.TaxConfig.TaxPercentage
        };
    }
    
    public ProductResponseDTO? GetById(int id)
    {
        var product = _repo.GetById(id);

        if (product == null)
            return null;

        return new ProductResponseDTO
        {
            Id = product.Id,
            Name = product.Name,
            Barcode = product.Barcode,
            Price = product.Price,
            CategoryName = product.Category.Name,
            Quantity = product.Inventory.Quantity,
            TaxPercentage = product.TaxConfig.TaxPercentage
        };
    }
    
    public bool UpdateInventory(int productId, int quantity)
    {
        var product = _repo.GetById(productId);

        if (product == null || product.Inventory.Quantity < quantity)
            return false;

        product.Inventory.Quantity -= quantity;

        _repo.Save();

        return true;
    }
    
    public List<ProductResponseDTO> GetLowStockProducts()
    {
        return _repo.GetLowStockProducts();
    }
    public List<ProductResponseDTO> GetByCategory(int categoryId)
    {
        return _repo.GetByCategory(categoryId)
            .Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                CategoryName = p.Category.Name,
                Quantity = p.Inventory.Quantity,
                TaxPercentage = p.TaxConfig.TaxPercentage
            })
            .ToList();
    }
    
    public string DeleteProduct(int id)
    {
        var product = _repo.GetById(id);

        if (product == null)
            throw new Exception("Product not found");

        _repo.Delete(product);

        return "Product deleted successfully";
    }

    private static string NormalizeBarcode(string barcode)
    {
        return new string(barcode.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidBarcode(string barcode)
        => SupportedBarcodeLengths.Contains(barcode.Length);

    private string GenerateBarcode()
    {
        var candidate = $"8901262{DateTime.UtcNow:HHmmss}";

        while (_repo.ExistsByBarcode(candidate))
        {
            candidate = $"8901262{Random.Shared.Next(100000, 999999)}";
        }

        return candidate;
    }
}
