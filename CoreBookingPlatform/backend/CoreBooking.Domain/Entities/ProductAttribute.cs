namespace CoreBooking.Domain.Entities;

public class ProductAttribute
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Product? Product { get; set; }
}
