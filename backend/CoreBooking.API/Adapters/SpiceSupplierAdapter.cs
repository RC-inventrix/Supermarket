using CoreBooking.Domain.Adapters;
using CoreBooking.Domain.Entities;
using System.Text.Json;

namespace CoreBooking.API.Services
{
    public class SpiceSupplierAdapter : IExternalProviderAdapter
    {
        private readonly HttpClient _httpClient;

        public SpiceSupplierAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(int providerId, int categoryId)
        {
            var response = await _httpClient.GetAsync("/api/supplier/catalog");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var externalProducts = JsonSerializer.Deserialize<List<ExternalProductDto>>(jsonString, options);

            return externalProducts.Select(ext => new Product
            {
                ProviderId = providerId,
                CategoryId = categoryId,
                ExternalProductId = ext.ExternalId,
                Name = ext.Name,
                Price = ext.BasePrice,
                AvailableQuantity = ext.AvailableQuantity // Quantity mapped here
            });
        }

        public async Task<int> CheckAvailabilityAsync(string externalProductId)
        {
            var response = await _httpClient.GetAsync($"/api/supplier/availability/{externalProductId}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.GetProperty("availableStock").GetInt32();
        }

        public async Task<string> PlaceOrderAsync(string externalProductId, int quantity)
        {
            var payload = new { ExternalId = externalProductId, Quantity = quantity };
            var response = await _httpClient.PostAsJsonAsync("/api/supplier/checkout", payload);

            if (!response.IsSuccessStatusCode) throw new Exception("External checkout failed.");

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);
            return doc.RootElement.GetProperty("confirmationCode").GetString();
        }

        private class ExternalProductDto
        {
            public string ExternalId { get; set; }
            public string Name { get; set; }
            public decimal BasePrice { get; set; }
            public int AvailableQuantity { get; set; } // Quantity field added here
        }
    }
}