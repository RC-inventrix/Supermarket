using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

        private SupplierMappingConfig GetConfig(Provider provider)
        {
            return JsonSerializer.Deserialize<SupplierMappingConfig>(
                string.IsNullOrWhiteSpace(provider.MappingConfigJson) ? "{}" : provider.MappingConfigJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new SupplierMappingConfig();
        }

        // --- Cleans the literal UI path (e.g., "[0].name" -> "name") so it works inside the loop
        private string GetRelativeItemPath(string arrayRootPath, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath)) return string.Empty;

            string pattern = string.IsNullOrWhiteSpace(arrayRootPath)
                ? @"^\[\d+\]\."
                : $@"^{Regex.Escape(arrayRootPath)}\[\d+\]\.";

            return Regex.Replace(fullPath, pattern, "");
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
                // Clean all paths before entering the loop
                var cleanIdPath = GetRelativeItemPath(mapping.ArrayRootPath, mapping.IdPath);
                var cleanNamePath = GetRelativeItemPath(mapping.ArrayRootPath, mapping.NamePath);
                var cleanPricePath = GetRelativeItemPath(mapping.ArrayRootPath, mapping.PricePath);
                var cleanQtyPath = GetRelativeItemPath(mapping.ArrayRootPath, mapping.CatalogQuantityPath);
                var cleanDescPath = GetRelativeItemPath(mapping.ArrayRootPath, mapping.DescriptionPath);

                // Keep track of mapped paths so we know which ones are "unmapped" for the EAV attributes
                var mappedPaths = new HashSet<string> { cleanIdPath, cleanNamePath, cleanPricePath, cleanQtyPath, cleanDescPath };

                foreach (var item in arrayNode)
                {
                    var externalId = GetStringValue(item, cleanIdPath) ?? Guid.NewGuid().ToString();
                    var name = GetStringValue(item, cleanNamePath) ?? "Unknown Product";
                    decimal.TryParse(GetStringValue(item, cleanPricePath), out decimal price);
                    int.TryParse(GetStringValue(item, cleanQtyPath), out int qty);
                    var description = GetStringValue(item, cleanDescPath);

                    var product = new Product
                    {
                        ProviderId = provider.Id,
                        CategoryId = provider.CategoryId,
                        ExternalProductId = externalId,
                        Name = name,
                        Price = price,
                        AvailableQuantity = qty
                    };

                    // --- 1. HANDLE CONTENT (Description) ---
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        product.Contents.Add(new ProductContent { ContentType = "Description", Data = description });
                    }

                    // --- 2. HANDLE EAV ATTRIBUTES (Unmapped Fields) ---
                    if (item is JsonObject jsonObj)
                    {
                        foreach (var kvp in jsonObj)
                        {
                            // If the key wasn't explicitly mapped, and it's a simple value (string/number/bool), save it as an attribute!
                            if (!mappedPaths.Contains(kvp.Key) && kvp.Value is JsonValue val)
                            {
                                product.Attributes.Add(new ProductAttribute { Name = kvp.Key, Value = val.ToString() });
                            }
                        }
                    }

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

            string? stockString = GetStringValue(rootNode, mapping.AvailabilityQuantityPath);
            int.TryParse(stockString, out int availableStock);

            return availableStock;
        }

        public async Task<string> PlaceOrderAsync(Provider provider, string externalProductId, int quantity)
        {
            var request = new { SupplierBaseUrl = provider.SupplierBaseUrl, Endpoint = provider.CheckoutEndpoint, ExternalProductId = externalProductId, Quantity = quantity };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/checkout", request);

            if (!response.IsSuccessStatusCode) throw new Exception("External checkout failed.");

            var mapping = GetConfig(provider);
            var rootNode = JsonNode.Parse(await response.Content.ReadAsStringAsync());

            string? confirmationCode = GetStringValue(rootNode, mapping.CheckoutConfirmationPath);

            return string.IsNullOrWhiteSpace(confirmationCode) ? "CONFIRMED" : confirmationCode;
        }

        // ---> THE MISSING FIX: Put the FetchSampleJsonAsync method back in! <---
        public async Task<string> FetchSampleJsonAsync(string supplierBaseUrl, string endpoint)
        {
            var request = new { SupplierBaseUrl = supplierBaseUrl, Endpoint = endpoint };
            var response = await _httpClient.PostAsJsonAsync("/api/gateway/fetch-sample", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch sample JSON from the external supplier.");
            }

            return await response.Content.ReadAsStringAsync();
        }
        // -------------------------------------------------------------------------

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

    public class SupplierMappingConfig
    {
        public string ArrayRootPath { get; set; } = string.Empty;
        public string IdPath { get; set; } = "id";
        public string NamePath { get; set; } = "name";
        public string PricePath { get; set; } = "price";
        public string CatalogQuantityPath { get; set; } = "quantity";
        public string DescriptionPath { get; set; } = "description";
        public string AvailabilityQuantityPath { get; set; } = "availableStock";
        public string CheckoutConfirmationPath { get; set; } = "confirmationCode";
    }
}