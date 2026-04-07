namespace CoreBooking.Domain.Entities;

public class ProductContent
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;

    public Product? Product { get; set; }
}
