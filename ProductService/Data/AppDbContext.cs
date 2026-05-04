using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<TaxConfig> TaxConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Barcode
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode)
            .IsUnique();

        // One-to-One: Product ↔ Inventory
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Inventory)
            .WithOne(i => i.Product)
            .HasForeignKey<Inventory>(i => i.ProductId);
    }
}