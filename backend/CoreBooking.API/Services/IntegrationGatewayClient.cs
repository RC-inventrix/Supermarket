using System.Net.Http.Json;
using CoreBooking.Domain.Entities;

namespace CoreBooking.API.Services
{
    // This acts as the bridge between your Core API and your new AdapterService.API
    public class IntegrationGatewayClient
    {
        private readonly HttpClient _httpClient;

        public IntegrationGatewayClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Product>> ImportProductsAsync(string supplierBaseUrl, int providerId, int categoryId)
        {
            var request = new { SupplierBaseUrl = supplierBaseUrl, Endpoint = "/api/supplier/catalog" };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/import", request);
            response.EnsureSuccessStatusCode();

            var externalProducts = await response.Content.ReadFromJsonAsync<List<ExternalProductDto>>();

            return externalProducts!.Select(ext => new Product
            {
                ProviderId = providerId,
                CategoryId = categoryId,
                ExternalProductId = ext.ExternalId,
                Name = ext.Name,
                Price = ext.BasePrice,
                AvailableQuantity = ext.AvailableQuantity
            }).ToList();
        }

        public async Task<int> CheckAvailabilityAsync(string supplierBaseUrl, string externalProductId)
        {
            var request = new { SupplierBaseUrl = supplierBaseUrl, Endpoint = "/api/supplier/availability", ExternalProductId = externalProductId };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/availability", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AvailabilityResponse>();
            return result!.AvailableStock;
        }

        public async Task<string> PlaceOrderAsync(string supplierBaseUrl, string externalProductId, int quantity)
        {
            var request = new { SupplierBaseUrl = supplierBaseUrl, Endpoint = "/api/supplier/checkout", ExternalProductId = externalProductId, Quantity = quantity };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/checkout", request);

            if (!response.IsSuccessStatusCode) throw new Exception("External checkout failed or stock is insufficient.");

            var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
            return result!.ConfirmationCode;
        }

        // Internal DTOs to deserialize the Gateway's JSON payload
        private class ExternalProductDto { public string ExternalId { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public decimal BasePrice { get; set; } public int AvailableQuantity { get; set; } }
        private class AvailabilityResponse { public int AvailableStock { get; set; } }
        private class CheckoutResponse { public string ConfirmationCode { get; set; } = string.Empty; }
    }
}