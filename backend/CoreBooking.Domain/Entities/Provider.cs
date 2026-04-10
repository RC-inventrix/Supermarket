namespace CoreBooking.Domain.Entities;

public class Provider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AdapterKey { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
