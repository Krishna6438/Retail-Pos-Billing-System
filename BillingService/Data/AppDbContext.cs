using Microsoft.EntityFrameworkCore;
using BillingService.Models;

namespace BillingService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillItem> BillItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Return> Returns { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Shift> Shifts { get; set; }
}