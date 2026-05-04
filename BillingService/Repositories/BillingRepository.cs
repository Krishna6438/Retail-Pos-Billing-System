using BillingService.Data;
using BillingService.DTOs;
using BillingService.DTOs.Analytics;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories;

public class BillingRepository
{
    private readonly AppDbContext _context;

    public BillingRepository(AppDbContext context)
    {
        _context = context;
    }

    public void AddBill(Bill bill)
    {
        _context.Bills.Add(bill);
        _context.SaveChanges();
    }
    
    public BillResponseDTO? GetBill(int billId)
    {
        var bill = _context.Bills
            .Include(b => b.Items)
            .FirstOrDefault(b => b.Id == billId);

        if (bill == null)
            return null;

        return new BillResponseDTO
        {
            BillId = bill.Id,
            TotalAmount = bill.TotalAmount,
            CreatedAt = bill.CreatedAt
        };
    }
    
    //  DAILY SALES
    public DailySalesDTO GetTodaySales()
    {
        var today = DateTime.UtcNow.Date;

        var bills = _context.Bills
            .Where(b => b.CreatedAt.Date == today)
            .ToList();

        return new DailySalesDTO
        {
            Date = today,
            TotalRevenue = bills.Sum(b => b.TotalAmount),
            TotalOrders = bills.Count
        };
    }

    //  TOTAL REVENUE
    public decimal GetTotalRevenue()
    {
        return _context.Bills.Sum(b => b.TotalAmount);
    }

    //  TOP PRODUCTS
    public List<TopProductDTO> GetTopProducts()
    {
        return _context.BillItems
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDTO
            {
                ProductId = g.Key,
                TotalQuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(5)
            .ToList();
    }

    //  BILLS BY USER
    public List<Bill> GetBillsByUser(int userId)
    {
        return _context.Bills
            .Where(b => b.UserId == userId)
            .ToList();
    }

    //  BILLS BY DATE
    public List<Bill> GetBillsByDate(DateTime date)
    {
        return _context.Bills
            .Where(b => b.CreatedAt.Date == date.Date)
            .ToList();
    }


    //  ADD COUPON
    public void AddCoupon(Coupon coupon)
    {
        _context.Coupons.Add(coupon);
        _context.SaveChanges();
    }

//  GET COUPON
    public Coupon? GetCoupon(string code)
    {
        return _context.Coupons.FirstOrDefault(c => c.Code == code);
    }

//  UPDATE BILL
    public void UpdateBill(Bill bill)
    {
        _context.SaveChanges();
    }
    
    public Bill? GetBillById(int billId)
    {
        return _context.Bills
            .Include(b => b.Payment)
            .FirstOrDefault(b => b.Id == billId);
    }
    
    public Cart? GetCartById(int cartId)
    {
        return _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.Id == cartId);
    }

    public int? GetCartOwnerId(int cartId)
    {
        return _context.Carts
            .Where(c => c.Id == cartId)
            .Select(c => (int?)c.UserId)
            .FirstOrDefault();
    }

    public int? GetBillOwnerId(int billId)
    {
        return _context.Bills
            .Where(b => b.Id == billId)
            .Select(b => (int?)b.UserId)
            .FirstOrDefault();
    }

    public void DeleteCart(int cartId)
    {
        var cart = _context.Carts
            .Include(c => c.Items)
            .FirstOrDefault(c => c.Id == cartId);

        if (cart == null)
            return;

        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);
        _context.SaveChanges();
    }
    
    //  ORDER HISTORY
    public List<Bill> GetBillsWithItemsByUser(int userId)
    {
        return _context.Bills
            .Include(b => b.Items)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId)
            .ToList();
    }

//  INVOICE
    public Bill? GetInvoice(int billId)
    {
        return _context.Bills
            .Include(b => b.Items)
            .Include(b => b.Payment)
            .FirstOrDefault(b => b.Id == billId);
    }


}
