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

        [HttpPost("import-products/{providerId}")]
        public async Task<IActionResult> ImportProducts(int providerId)
        {
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) return NotFound("Provider not found.");

            // Pass the entire provider (with BaseUrl, Endpoints, and Mappings)
            var importedProducts = await _gatewayClient.ImportProductsAsync(provider);

            // Fetch existing products, explicitly including Attributes and Content so we can update them
            var existingProducts = await _context.Products
                .Include(p => p.Attributes)
                .Include(p => p.Contents)
                .Where(p => p.ProviderId == providerId)
                .ToListAsync();

            int addedCount = 0;
            int updatedCount = 0;

            foreach (var importedProduct in importedProducts)
            {
                var existingProduct = existingProducts.FirstOrDefault(p => p.ExternalProductId == importedProduct.ExternalProductId);
                if (existingProduct != null)
                {
                    existingProduct.Name = importedProduct.Name;
                    existingProduct.Price = importedProduct.Price;
                    existingProduct.AvailableQuantity = importedProduct.AvailableQuantity;

                    // Clear and replace dynamic Attributes & Content to keep them fresh
                    _context.ProductAttributes.RemoveRange(existingProduct.Attributes);
                    _context.ProductContents.RemoveRange(existingProduct.Contents);

                    existingProduct.Attributes = importedProduct.Attributes;
                    existingProduct.Contents = importedProduct.Contents;

                    updatedCount++;
                }
                else
                {
                    _context.Products.Add(importedProduct);
                    addedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Dynamic Sync Complete", Added = addedCount, Updated = updatedCount });
        }

        [HttpPut("sync-availability/{productId}")]
        public async Task<IActionResult> SyncProductAvailability(int productId)
        {
            var product = await _context.Products.Include(p => p.Provider).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || product.Provider == null) return NotFound("Product or Provider not found.");

            product.AvailableQuantity = await _gatewayClient.CheckAvailabilityAsync(product.Provider, product.ExternalProductId);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully synced availability for '{product.Name}'.", LiveStock = product.AvailableQuantity });
        }
    }
}