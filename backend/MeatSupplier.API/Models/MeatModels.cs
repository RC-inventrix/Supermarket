namespace MeatSupplier.API.Models
{
    // Changed to a class so we can modify the AvailableQuantity during checkout
    public class ExternalProduct
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public string Description { get; set; }
        public int AvailableQuantity { get; set; } // New field added
    }

    // What the Core Application sends when placing an order
    public record CheckoutRequest(string ExternalId, int Quantity);

    // What the Meat API returns after a successful order
    public record CheckoutResponse(bool IsSuccess, string ConfirmationCode, string Message);
}