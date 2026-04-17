using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CoreBooking.Domain.Entities;

namespace CoreBooking.API.Services
{
    public class IntegrationGatewayClient
    {
        private readonly HttpClient _httpClient;

        public IntegrationGatewayClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- HELPER: Extracts the config from the database string ---
        private SupplierMappingConfig GetConfig(Provider provider)
        {
            return JsonSerializer.Deserialize<SupplierMappingConfig>(
                string.IsNullOrWhiteSpace(provider.MappingConfigJson) ? "{}" : provider.MappingConfigJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new SupplierMappingConfig();
        }

        public async Task<List<Product>> ImportProductsAsync(Provider provider)
        {
            var request = new { SupplierBaseUrl = provider.SupplierBaseUrl, Endpoint = provider.CatalogEndpoint };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/import", request);
            response.EnsureSuccessStatusCode();

            var mapping = GetConfig(provider);
            var rootNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            var arrayNode = GetNodeByPath(rootNode, mapping.ArrayRootPath)?.AsArray();

            var products = new List<Product>();

            if (arrayNode != null)
            {
                foreach (var item in arrayNode)
                {
                    var externalId = GetStringValue(item, mapping.IdPath) ?? Guid.NewGuid().ToString();
                    var name = GetStringValue(item, mapping.NamePath) ?? "Unknown Product";
                    decimal.TryParse(GetStringValue(item, mapping.PricePath), out decimal price);

                    // CHANGED: Uses the specific Catalog path
                    int.TryParse(GetStringValue(item, mapping.CatalogQuantityPath), out int qty);

                    var product = new Product
                    {
                        ProviderId = provider.Id,
                        CategoryId = provider.CategoryId,
                        ExternalProductId = externalId,
                        Name = name,
                        Price = price,
                        AvailableQuantity = qty
                    };

                    products.Add(product);
                }
            }
            return products;
        }

        public async Task<int> CheckAvailabilityAsync(Provider provider, string externalProductId)
        {
            var request = new { SupplierBaseUrl = provider.SupplierBaseUrl, Endpoint = provider.AvailabilityEndpoint, ExternalProductId = externalProductId };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/availability", request);
            response.EnsureSuccessStatusCode();

            var mapping = GetConfig(provider);
            var rootNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());

            // CHANGED: Dynamically parses the Availability response!
            string? stockString = GetStringValue(rootNode, mapping.AvailabilityQuantityPath);
            int.TryParse(stockString, out int availableStock);

            return availableStock;
        }

        // Add this new method to fetch the raw sample JSON
        public async Task<string> FetchSampleJsonAsync(string supplierBaseUrl, string endpoint)
        {
            var request = new { SupplierBaseUrl = supplierBaseUrl, Endpoint = endpoint };
            // Sends the request to your AdapterService gateway
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/fetch-sample", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch sample JSON from the external supplier.");
            }

            // Return the raw JSON string so React can render the visual tree
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> PlaceOrderAsync(Provider provider, string externalProductId, int quantity)
        {
            var request = new { SupplierBaseUrl = provider.SupplierBaseUrl, Endpoint = provider.CheckoutEndpoint, ExternalProductId = externalProductId, Quantity = quantity };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/checkout", request);

            if (!response.IsSuccessStatusCode) throw new Exception("External checkout failed.");

            var mapping = GetConfig(provider);
            var rootNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());

            // CHANGED: Dynamically parses the Checkout response!
            string? confirmationCode = GetStringValue(rootNode, mapping.CheckoutConfirmationPath);

            return string.IsNullOrWhiteSpace(confirmationCode) ? "CONFIRMED" : confirmationCode;
        }

        // --- JSON DOT-NOTATION HELPER METHODS ---
        private JsonNode? GetNodeByPath(JsonNode? current, string path)
        {
            if (current == null || string.IsNullOrEmpty(path)) return current;
            foreach (var step in path.Split('.'))
            {
                if (current == null) return null;
                current = current[step];
            }
            return current;
        }

        private string? GetStringValue(JsonNode? node, string path) => GetNodeByPath(node, path)?.ToString();
    }

    // --- EXPANDED CONFIGURATION MODEL ---
    public class SupplierMappingConfig
    {
        // 1. Catalog Import Paths
        public string ArrayRootPath { get; set; } = string.Empty;
        public string IdPath { get; set; } = "id";
        public string NamePath { get; set; } = "name";
        public string PricePath { get; set; } = "price";
        public string CatalogQuantityPath { get; set; } = "quantity"; // Used for bulk import
        public string DescriptionPath { get; set; } = "description";

        // 2. Availability Check Paths
        public string AvailabilityQuantityPath { get; set; } = "availableStock"; // Used for real-time checks

        // 3. Checkout Paths
        public string CheckoutConfirmationPath { get; set; } = "confirmationCode"; // Used after checkout
    }
}