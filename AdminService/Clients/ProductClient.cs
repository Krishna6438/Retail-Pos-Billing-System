using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdminService.Configuration;
using Microsoft.Extensions.Options;

namespace AdminService.Clients;

public class ProductClient
{
    private readonly HttpClient _http;
    private readonly string _productServiceBaseUrl;

    public ProductClient(HttpClient http, IOptions<ServiceEndpointsOptions> endpoints)
    {
        _http = http;
        _productServiceBaseUrl = endpoints.Value.ProductServiceBaseUrl.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(_productServiceBaseUrl))
            throw new InvalidOperationException("ServiceEndpoints:ProductServiceBaseUrl is not configured.");
    }

    public async Task<List<object>> GetLowStock(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await _http.GetFromJsonAsync<List<object>>(
            $"{_productServiceBaseUrl}/products/low-stock"
        ) ?? new List<object>();
    }
}
