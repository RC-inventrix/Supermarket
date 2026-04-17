using CoreBooking.API.Services;
using CoreBooking.Domain.Entities;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;
        private readonly IntegrationGatewayClient _gatewayClient;

        public SuppliersController(CoreBookingDbContext context, IntegrationGatewayClient gatewayClient)
        {
            _context = context;
            _gatewayClient = gatewayClient;
        }

        // GET: /api/suppliers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var providers = await _context.Providers.ToListAsync();

                var result = providers.Select(p =>
                {
                    object? parsedMapping = null;

                    if (!string.IsNullOrWhiteSpace(p.MappingConfigJson))
                    {
                        try
                        {
                            parsedMapping = JsonSerializer.Deserialize<object>(p.MappingConfigJson);
                        }
                        catch
                        {
                            // Silently ignore invalid JSON from old records
                        }
                    }

                    return new
                    {
                        Id = p.Id,
                        Name = p.Name ?? "Unknown Supplier",
                        CategoryId = p.CategoryId,
                        SupplierBaseUrl = p.SupplierBaseUrl ?? string.Empty,
                        CatalogEndpoint = p.CatalogEndpoint ?? string.Empty,
                        AvailabilityEndpoint = p.AvailabilityEndpoint ?? string.Empty,
                        CheckoutEndpoint = p.CheckoutEndpoint ?? string.Empty,
                        IsActive = p.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        MappingConfig = parsedMapping
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to load suppliers from database.", Details = ex.Message });
            }
        }

        // POST: /api/suppliers 
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto request)
        {
            try
            {
                var provider = new Provider
                {
                    Name = request.Name,
                    CategoryId = request.CategoryId,
                    SupplierBaseUrl = request.SupplierBaseUrl,
                    CatalogEndpoint = request.CatalogEndpoint,
                    AvailabilityEndpoint = request.AvailabilityEndpoint,
                    CheckoutEndpoint = request.CheckoutEndpoint,
                    MappingConfigJson = JsonSerializer.Serialize(request.MappingConfig),
                    IsActive = true
                };

                _context.Providers.Add(provider);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Supplier created successfully", Id = provider.Id });
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { Message = "Failed to save supplier.", Details = detailedError });
            }
        }

        // DELETE: /api/suppliers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null) return NotFound("Supplier not found.");

            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Supplier deleted successfully." });
        }

        // POST: /api/suppliers/{id}/import 
        [HttpPost("{id}/import")]
        public async Task<IActionResult> ImportProducts(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null) return NotFound("Supplier not found.");

            try
            {
                var importedProducts = await _gatewayClient.ImportProductsAsync(provider);

                var existingProducts = await _context.Products
                    .Include(p => p.Attributes)
                    .Include(p => p.Contents)
                    .Where(p => p.ProviderId == id)
                    .ToListAsync();

                int addedCount = 0;
                int updatedCount = 0;

                foreach (var importedProduct in importedProducts)
                {
                    var existingProduct = existingProducts.FirstOrDefault(p => p.ExternalProductId == importedProduct.ExternalProductId);
                    if (existingProduct != null)
                    {
                        // Update existing product
                        existingProduct.Name = importedProduct.Name;
                        existingProduct.Price = importedProduct.Price;
                        existingProduct.AvailableQuantity = importedProduct.AvailableQuantity;

                        _context.ProductAttributes.RemoveRange(existingProduct.Attributes);
                        _context.ProductContents.RemoveRange(existingProduct.Contents);

                        existingProduct.Attributes = importedProduct.Attributes;
                        existingProduct.Contents = importedProduct.Contents;
                        updatedCount++;
                    }
                    else
                    {
                        // Add new product
                        _context.Products.Add(importedProduct);
                        addedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { imported = addedCount + updatedCount, Message = "Import successful." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Import failed.", Details = ex.Message });
            }
        }

        // ---> NEW FIX: Sync Availability Endpoint <---
        // POST: /api/suppliers/{id}/sync-availability
        [HttpPost("{id}/sync-availability")]
        public async Task<IActionResult> SyncAvailability(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null) return NotFound("Supplier not found.");

            try
            {
                var existingProducts = await _context.Products
                    .Where(p => p.ProviderId == id)
                    .ToListAsync();

                int updatedCount = 0;

                // Loop through all existing products and ask the Gateway for live stock
                foreach (var product in existingProducts)
                {
                    var liveStock = await _gatewayClient.CheckAvailabilityAsync(provider, product.ExternalProductId);

                    // Only update and save if the stock actually changed
                    if (product.AvailableQuantity != liveStock)
                    {
                        product.AvailableQuantity = liveStock;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new { synced = existingProducts.Count, updated = updatedCount, Message = "Availability sync successful." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sync failed.", Details = ex.Message });
            }
        }
        // ---------------------------------------------

        // POST: /api/suppliers/fetch-sample
        [HttpPost("fetch-sample")]
        public async Task<IActionResult> FetchSample([FromBody] FetchSampleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BaseUrl) || string.IsNullOrWhiteSpace(request.Endpoint))
            {
                return BadRequest("BaseUrl and Endpoint are required.");
            }

            try
            {
                var jsonString = await _gatewayClient.FetchSampleJsonAsync(request.BaseUrl, request.Endpoint);
                return Content(jsonString, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }

    // --- DTOs ---
    public class FetchSampleRequest
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public class CreateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string SupplierBaseUrl { get; set; } = string.Empty;
        public string CatalogEndpoint { get; set; } = string.Empty;
        public string AvailabilityEndpoint { get; set; } = string.Empty;
        public string CheckoutEndpoint { get; set; } = string.Empty;
        public object? MappingConfig { get; set; }
    }
}