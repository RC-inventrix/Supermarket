using CoreBooking.Domain.Entities;

namespace CoreBooking.Domain.Adapters
{
    public interface IExternalProviderAdapter
    {
        // Fetches external products and translates them into our Domain Product entity
        Task<IEnumerable<Product>> GetProductsAsync(int providerId, int categoryId);

        // Returns the available stock quantity
        Task<int> CheckAvailabilityAsync(string externalProductId);

        // Performs the checkout and returns the ExternalBookingReference (e.g., "MEAT-CONF-123")
        Task<string> PlaceOrderAsync(string externalProductId, int quantity);
    }
}