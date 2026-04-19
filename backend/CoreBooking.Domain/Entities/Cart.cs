namespace CoreBooking.Domain.Entities;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // NEW: Stores the calculated total of all items in the cart
    public decimal TotalPrice { get; set; }

    //public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}