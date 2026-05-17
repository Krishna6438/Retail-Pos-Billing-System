using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdminService.DTOs;
using AdminService.Configuration;
using Microsoft.Extensions.Options;

namespace AdminService.Clients;

public class BillingClient
{
    private readonly HttpClient _http;
    private readonly string _billingServiceBaseUrl;

    public BillingClient(HttpClient http, IOptions<ServiceEndpointsOptions> endpoints)
    {
        _http = http;
        _billingServiceBaseUrl = endpoints.Value.BillingServiceBaseUrl.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(_billingServiceBaseUrl))
            throw new InvalidOperationException("ServiceEndpoints:BillingServiceBaseUrl is not configured.");
    }

    //  TOTAL REVENUE
    public async Task<decimal> GetRevenue(string token)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return await _http.GetFromJsonAsync<decimal>(
                $"{_billingServiceBaseUrl}/billing/analytics/revenue"
            );
        }
        catch (HttpRequestException)
        {
            return 0;
        }
    }

    //  TOTAL ORDERS (FIXED )
    public async Task<int> GetTotalOrders(string token)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var data = await _http.GetFromJsonAsync<TodaySalesDTO>(
                $"{_billingServiceBaseUrl}/billing/analytics/today"
            );

            return data?.TotalOrders ?? 0;
        }
        catch (HttpRequestException)
        {
            return 0;
        }
    }

    // ✅ TOP PRODUCTS
    public async Task<List<object>> GetTopProducts(string token)
    {
        try
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return await _http.GetFromJsonAsync<List<object>>(
                $"{_billingServiceBaseUrl}/billing/analytics/top-products"
            ) ?? new List<object>();
        }
        catch (HttpRequestException)
        {
            return new List<object>();
        }
    }
}
