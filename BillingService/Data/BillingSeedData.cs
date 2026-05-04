using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public static class BillingSeedData
{
    public static void Seed(AppDbContext context)
    {
        context.Database.Migrate();

        if (!context.Coupons.Any())
        {
            context.Coupons.AddRange(
                new Coupon
                {
                    Code = "WELCOME10",
                    DiscountPercentage = 10m,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6)
                },
                new Coupon
                {
                    Code = "WEEKEND15",
                    DiscountPercentage = 15m,
                    ExpiryDate = DateTime.UtcNow.AddMonths(3)
                },
                new Coupon
                {
                    Code = "BULK20",
                    DiscountPercentage = 20m,
                    ExpiryDate = DateTime.UtcNow.AddMonths(2)
                });
        }

        if (!context.Bills.Any())
        {
            context.Bills.AddRange(
                CreateBill(2, "UPI", "Completed", DateTime.UtcNow.AddDays(-2), new[]
                {
                    CreateBillItem(1, 2, 68m, 6.8m),
                    CreateBillItem(4, 3, 25m, 9m),
                    CreateBillItem(6, 2, 40m, 9.6m)
                }),
                CreateBill(3, "Card", "Completed", DateTime.UtcNow.AddDays(-1), new[]
                {
                    CreateBillItem(8, 1, 268m, 48.24m),
                    CreateBillItem(9, 1, 118m, 21.24m),
                    CreateBillItem(11, 2, 99m, 35.64m)
                }),
                CreateBill(2, "Cash", "Pending", DateTime.UtcNow.AddHours(-8), new[]
                {
                    CreateBillItem(2, 1, 289m, 14.45m),
                    CreateBillItem(3, 2, 28m, 2.8m),
                    CreateBillItem(12, 4, 95m, 45.6m)
                }));
        }

        context.SaveChanges();
    }

    private static Bill CreateBill(int userId, string method, string status, DateTime createdAt, IEnumerable<BillItem> items)
    {
        var itemList = items.ToList();
        var total = itemList.Sum(item => (item.Price * item.Quantity) + item.TaxAmount);

        return new Bill
        {
            UserId = userId,
            CreatedAt = createdAt,
            TotalAmount = total,
            Items = itemList,
            Payment = new Payment
            {
                Method = method,
                Amount = total,
                Status = status
            }
        };
    }

    private static BillItem CreateBillItem(int productId, int quantity, decimal unitPrice, decimal lineTaxAmount)
    {
        return new BillItem
        {
            ProductId = productId,
            Quantity = quantity,
            Price = unitPrice,
            TaxAmount = lineTaxAmount
        };
    }
}
