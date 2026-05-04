using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public static class ProductSeedData
{
    public static void Seed(AppDbContext context)
    {
        context.Database.Migrate();

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Grocery" },
                new Category { Name = "Beverages" },
                new Category { Name = "Snacks" },
                new Category { Name = "Personal Care" },
                new Category { Name = "Household" },
                new Category { Name = "Stationery" });
        }

        if (!context.TaxConfigs.Any())
        {
            context.TaxConfigs.AddRange(
                new TaxConfig { Name = "GST 5%", TaxPercentage = 5m },
                new TaxConfig { Name = "GST 12%", TaxPercentage = 12m },
                new TaxConfig { Name = "GST 18%", TaxPercentage = 18m });
        }

        context.SaveChanges();

        var categories = context.Categories.AsNoTracking().ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        var taxes = context.TaxConfigs.AsNoTracking().ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var products = new[]
        {
            CreateProduct("Amul Taaza Milk 1L", "8901262000013", 68m, "Grocery", "GST 5%", 48, categories, taxes),
            CreateProduct("Aashirvaad Atta 5kg", "8901262000020", 289m, "Grocery", "GST 5%", 26, categories, taxes),
            CreateProduct("Tata Salt 1kg", "8901262000037", 28m, "Grocery", "GST 5%", 72, categories, taxes),
            CreateProduct("Parle-G Family Pack", "8901262000044", 25m, "Snacks", "GST 12%", 110, categories, taxes),
            CreateProduct("Kurkure Masala Munch", "8901262000051", 20m, "Snacks", "GST 12%", 84, categories, taxes),
            CreateProduct("Coca-Cola 750ml", "8901262000068", 40m, "Beverages", "GST 12%", 55, categories, taxes),
            CreateProduct("Maaza Mango Drink 600ml", "8901262000075", 38m, "Beverages", "GST 12%", 34, categories, taxes),
            CreateProduct("Dove Shampoo 340ml", "8901262000082", 268m, "Personal Care", "GST 18%", 19, categories, taxes),
            CreateProduct("Colgate Strong Teeth 200g", "8901262000099", 118m, "Personal Care", "GST 18%", 41, categories, taxes),
            CreateProduct("Surf Excel Easy Wash 1kg", "8901262000105", 152m, "Household", "GST 18%", 22, categories, taxes),
            CreateProduct("Harpic Power Plus 500ml", "8901262000112", 99m, "Household", "GST 18%", 17, categories, taxes),
            CreateProduct("Classmate Notebook 240 Pages", "8901262000129", 95m, "Stationery", "GST 12%", 63, categories, taxes),
            CreateProduct("Maggi 2-Minute Noodles 70g", "8901058014235", 14m, "Snacks", "GST 12%", 150, categories, taxes),
            CreateProduct("Nescafe Classic 50g", "8901058000211", 165m, "Beverages", "GST 12%", 40, categories, taxes)
        };

        var existingBarcodes = context.Products.Select(p => p.Barcode).ToHashSet();
        var productsToAdd = products.Where(p => !existingBarcodes.Contains(p.Barcode)).ToList();

        if (productsToAdd.Any())
        {
            context.Products.AddRange(productsToAdd);
            context.SaveChanges();
        }
    }

    private static Product CreateProduct(
        string name,
        string barcode,
        decimal price,
        string categoryName,
        string taxName,
        int quantity,
        IDictionary<string, Category> categories,
        IDictionary<string, TaxConfig> taxes)
    {
        return new Product
        {
            Name = name,
            Barcode = barcode,
            Price = price,
            CategoryId = categories[categoryName].Id,
            TaxConfigId = taxes[taxName].Id,
            Inventory = new Inventory
            {
                Quantity = quantity
            }
        };
    }
}
