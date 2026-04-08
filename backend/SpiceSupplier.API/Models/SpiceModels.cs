namespace SpiceSupplier.API.Models
{
    // Class format to allow mutable quantities during checkout
    public class ExternalProduct
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public string Description { get; set; }
        public int AvailableQuantity { get; set; }
    }

    // What the Core Application sends when placing an order
    public record CheckoutRequest(string ExternalId, int Quantity);

    // What the Spice API returns after a successful order
    public record CheckoutResponse(bool IsSuccess, string ConfirmationCode, string Message);
}