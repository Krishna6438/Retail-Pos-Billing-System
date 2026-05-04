using AdminService.Clients;
using AdminService.DTOs;

namespace AdminService.Services;

public class AdminService
{
    private readonly BillingClient _billing;
    private readonly ProductClient _product;

    public AdminService(BillingClient billing, ProductClient product)
    {
        _billing = billing;
        _product = product;
    }

    public async Task<DashboardDTO> GetDashboard(string token)
    {
        var revenue = await _billing.GetRevenue(token);
        var orders = await _billing.GetTotalOrders(token);
        var topProducts = await _billing.GetTopProducts(token);
        var lowStock = await _product.GetLowStock(token);

        return new DashboardDTO
        {
            TotalRevenue = revenue,
            TotalOrders = orders,
            TopProducts = topProducts,
            LowStockProducts = lowStock
        };
    }
}