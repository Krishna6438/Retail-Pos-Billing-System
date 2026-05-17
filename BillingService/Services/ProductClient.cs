using System.Net.Http.Json;
using BillingService.Configuration;
using Microsoft.Extensions.Options;

namespace BillingService.Services;

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

    public async Task<ProductResponse?> GetProduct(int productId, string token)
    {
        try
        {
            _http.DefaultRequestHeaders.Clear();

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    token.Replace("Bearer ", "")
                );

            var response = await _http.GetAsync($"{_productServiceBaseUrl}/products/id/{productId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductResponse>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    //  Update inventory
    public async Task<bool> UpdateInventory(int productId, int quantity, string token)
    {
        _http.DefaultRequestHeaders.Clear();

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token.Replace("Bearer ", "")
            );

        var response = await _http.PutAsync(
            $"{_productServiceBaseUrl}/products/inventory/{productId}?quantity={quantity}",
            null
        );

        return response.IsSuccessStatusCode;
    }
}

//  Response model
public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TaxPercentage { get; set; }
}
