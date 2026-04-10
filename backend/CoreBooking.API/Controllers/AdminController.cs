using CoreBooking.Domain.Adapters;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Added for LINQ queries
using System.Threading.Tasks;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;
        private readonly Func<string, IExternalProviderAdapter> _adapterFactory;

        public AdminController(CoreBookingDbContext context, Func<string, IExternalProviderAdapter> adapterFactory)
        {
            _context = context;
            _adapterFactory = adapterFactory;
        }

        [HttpPost("import-products/{providerId}")]
        public async Task<IActionResult> ImportProducts(int providerId)
        {
            // 1. Find the provider in our DB
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) return NotFound("Provider not found.");

            int categoryId = provider.CategoryId;

            // 2. Instantiate the correct adapter using the factory
            var adapter = _adapterFactory(provider.AdapterKey);

            // 3. Fetch and translate the fresh data from the external supplier
            var importedProducts = await adapter.GetProductsAsync(providerId, categoryId);

            // --- ADDED CHANGE BEGIN: The Upsert Logic ---

            // Fetch all EXISTING products for this provider from our local database
            var existingProducts = await _context.Products
                .Where(p => p.ProviderId == providerId )
                .ToListAsync();

            int addedCount = 0;
            int updatedCount = 0;

            // Loop through the fresh data from the supplier
            foreach (var importedProduct in importedProducts)
            {
                // Check if this product already exists locally based on the ExternalProductId
                var existingProduct = existingProducts
                    .FirstOrDefault(p => p.ExternalProductId == importedProduct.ExternalProductId);

                if (existingProduct != null)
                {
                    // UPDATE CHECK: Only update if at least one tracked field has actually changed
                    if (existingProduct.Name != importedProduct.Name ||
                        existingProduct.Price != importedProduct.Price ||
                        existingProduct.AvailableQuantity != importedProduct.AvailableQuantity)
                    {
                        // Apply the new values
                        existingProduct.Name = importedProduct.Name;
                        existingProduct.Price = importedProduct.Price;
                        existingProduct.AvailableQuantity = importedProduct.AvailableQuantity;

                        // Only count it as an update if changes were actually made
                        updatedCount++;
                    }
                    // If the product exists but all values are identical, the code safely does nothing.
                }
                else
                {
                    // INSERT: This is a brand new product.
                    _context.Products.Add(importedProduct);
                    addedCount++;
                }
            }

            // Save all updates and new inserts to the database
            await _context.SaveChangesAsync();

            // --- ADDED CHANGE END ---

            return Ok(new
            {
                Message = "Sync Complete",
                Added = addedCount,
                Updated = updatedCount
            });
        }

        [HttpPut("sync-availability/{productId}")]
        public async Task<IActionResult> SyncProductAvailability(int productId)
        {
            // 1. Find the product in our DB, explicitly including the Provider so we can access its AdapterKey
            var product = await _context.Products
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null || product.Provider == null)
            {
                return NotFound("Product or associated Provider not found.");
            }

            // 2. Instantiate the correct adapter for this specific product using the factory
            var adapter = _adapterFactory(product.Provider.AdapterKey);

            // 3. Fetch the real-time availability from the external supplier via the adapter
            var availabilityResult = await adapter.CheckAvailabilityAsync(product.ExternalProductId);

            // 4. Update the local product's AvailableQuantity with the fresh data from the supplier
            product.AvailableQuantity = availabilityResult;

            // 5. Save the updated quantity to our Core SQL Database
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Successfully synced availability for '{product.Name}'.",
                LiveStock = product.AvailableQuantity
            });
        }
    }
}