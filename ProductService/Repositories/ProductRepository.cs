using ProductService.Data;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;
using ProductService.DTOs;

namespace ProductService.Repositories;

public class ProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public List<Product> GetAll()
    {
        return _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Include(p => p.TaxConfig)
            .ToList();
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public Product? GetByBarcode(string barcode)
    {
        return _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Include(p => p.TaxConfig)
            .FirstOrDefault(p => p.Barcode == barcode);
    }
    
    public Product? GetById(int id)
    {
        return _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Include(p => p.TaxConfig)
            .FirstOrDefault(p => p.Id == id);
    }
    
    public void Save()
    {
        _context.SaveChanges();
    }
    
    public Category? GetCategoryByName(string name)
    {
        return _context.Categories.FirstOrDefault(c => c.Name == name);
    }

    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
        _context.SaveChanges();
    }

    public TaxConfig? GetTaxByPercentage(decimal percentage)
    {
        return _context.TaxConfigs.FirstOrDefault(t => t.TaxPercentage == percentage);
    }

    public void AddTax(TaxConfig tax)
    {
        _context.TaxConfigs.Add(tax);
        _context.SaveChanges();
    }
    
    public List<ProductResponseDTO> GetLowStockProducts()
    {
        return _context.Products
            .Include(p => p.Inventory)
            .Include(p => p.Category)
            .Include(p => p.TaxConfig)
            .Where(p => p.Inventory.Quantity < 10)
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
    public List<Product> GetByCategory(int categoryId)
    {
        return _context.Products
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Include(p => p.TaxConfig)
            .Where(p => p.CategoryId == categoryId)
            .ToList();
    }
    
    public void Delete(Product product)
    {
        _context.Products.Remove(product);
        _context.SaveChanges();
    }

    public bool ExistsByBarcode(string barcode)
    {
        return _context.Products.Any(p => p.Barcode == barcode);
    }
}
