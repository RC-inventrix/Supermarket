using CoreBooking.API.Services;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;
        private readonly IntegrationGatewayClient _gatewayClient;

        public AdminController(CoreBookingDbContext context, IntegrationGatewayClient gatewayClient)
        {
            _context = context;
            _gatewayClient = gatewayClient;
        }

        // Temporary helper until your UI is built. 
        // In the future, the Provider table will just save the URL directly!
        private string GetSupplierUrl(string adapterKey) => adapterKey switch
        {
            "Meat" => "http://meatsupplier-api:8080",
            "Veggie" => "http://veggiesupplier-api:8080",
            "Spice" => "http://spicesupplier-api:8080",
            _ => adapterKey
        };

        [HttpPost("import-products/{providerId}")]
        public async Task<IActionResult> ImportProducts(int providerId)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) return NotFound("Provider not found.");

            string supplierUrl = GetSupplierUrl(provider.AdapterKey);

            // CHANGED: Using the Gateway Client instead of the Factory
            var importedProducts = await _gatewayClient.ImportProductsAsync(supplierUrl, providerId, provider.CategoryId);

            var existingProducts = await _context.Products.Where(p => p.ProviderId == providerId).ToListAsync();
            int addedCount = 0;
            int updatedCount = 0;

            foreach (var importedProduct in importedProducts)
            {
                var existingProduct = existingProducts.FirstOrDefault(p => p.ExternalProductId == importedProduct.ExternalProductId);
                if (existingProduct != null)
                {
                    if (existingProduct.Name != importedProduct.Name ||
                        existingProduct.Price != importedProduct.Price ||
                        existingProduct.AvailableQuantity != importedProduct.AvailableQuantity)
                    {
                        existingProduct.Name = importedProduct.Name;
                        existingProduct.Price = importedProduct.Price;
                        existingProduct.AvailableQuantity = importedProduct.AvailableQuantity;
                        updatedCount++;
                    }
                }
                else
                {
                    _context.Products.Add(importedProduct);
                    addedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Sync Complete", Added = addedCount, Updated = updatedCount });
        }

        [HttpPut("sync-availability/{productId}")]
        public async Task<IActionResult> SyncProductAvailability(int productId)
        {
            var product = await _context.Products.Include(p => p.Provider).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || product.Provider == null) return NotFound("Product or Provider not found.");

            string supplierUrl = GetSupplierUrl(product.Provider.AdapterKey);

            // CHANGED: Using the Gateway Client
            product.AvailableQuantity = await _gatewayClient.CheckAvailabilityAsync(supplierUrl, product.ExternalProductId);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully synced availability for '{product.Name}'.", LiveStock = product.AvailableQuantity });
        }
    }
}