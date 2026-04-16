namespace CoreBooking.Domain.Entities;

public class Provider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // --- NEW DYNAMIC INTEGRATION CONFIGURATION ---
    public string SupplierBaseUrl { get; set; } = string.Empty;
    public string CatalogEndpoint { get; set; } = string.Empty;
    public string AvailabilityEndpoint { get; set; } = string.Empty;
    public string CheckoutEndpoint { get; set; } = string.Empty;

    // Stores the JSON dot-notation paths as a serialized JSON string 
    // (e.g., {"IdPath": "product.id", "NamePath": "details.title", ...})
    public string MappingConfigJson { get; set; } = string.Empty;
    // ---------------------------------------------

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}