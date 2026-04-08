namespace CoreBooking.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public int CategoryId { get; set; }
    public string ExternalProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // New property to cache the external stock level locally during import
    public int AvailableQuantity { get; set; }

    public Provider? Provider { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public ICollection<ProductContent> Contents { get; set; } = new List<ProductContent>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}