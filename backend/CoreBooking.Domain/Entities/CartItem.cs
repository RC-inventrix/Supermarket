namespace CoreBooking.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    // NEW: Locks in the price of the product when added to the cart
    public decimal UnitPrice { get; set; }

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}