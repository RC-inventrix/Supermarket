using CoreBooking.Domain.Entities;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;

        public ProductsController(CoreBookingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Provider)
                    .Include(p => p.Contents)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.ProviderId,
                        p.CategoryId,
                        p.ExternalProductId,
                        p.Name,
                        p.Price,
                        p.AvailableQuantity,
                        CategoryName = p.Category != null ? p.Category.Name : "—",
                        SupplierName = p.Provider != null ? p.Provider.Name : "—",
                        Description = p.Contents.FirstOrDefault(c => c.ContentType == "Description") != null
                                    ? p.Contents.FirstOrDefault(c => c.ContentType == "Description")!.Data
                                    : string.Empty,
                        IsManual = p.ExternalProductId.StartsWith("MANUAL-")
                    })
                    .ToListAsync();

                return Ok(new { items = products, totalCount, totalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch products.", Details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto request)
        {
            try
            {
                int finalProviderId = request.ProviderId ?? 0;

                // NEW FIX: If no existing ProviderId is passed but a NewProviderName exists, create it!
                if (finalProviderId == 0 && !string.IsNullOrWhiteSpace(request.NewProviderName))
                {
                    var newProvider = new Provider
                    {
                        Name = request.NewProviderName,
                        CategoryId = request.CategoryId,
                        IsActive = true,
                        SupplierBaseUrl = string.Empty,
                        CatalogEndpoint = string.Empty,
                        AvailabilityEndpoint = string.Empty,
                        CheckoutEndpoint = string.Empty,
                        MappingConfigJson = "{}"
                    };

                    _context.Providers.Add(newProvider);
                    await _context.SaveChangesAsync(); // Save immediately to generate the new ID
                    finalProviderId = newProvider.Id;
                }

                if (finalProviderId == 0) return BadRequest("A valid supplier is required.");

                var product = new Product
                {
                    Name = request.Name,
                    Price = request.Price,
                    AvailableQuantity = request.AvailableQuantity,
                    CategoryId = request.CategoryId,
                    ProviderId = finalProviderId,
                    ExternalProductId = "MANUAL-" + Guid.NewGuid().ToString()
                };

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    product.Contents.Add(new ProductContent { ContentType = "Description", Data = request.Description });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Product created successfully", Id = product.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create product.", Details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto request)
        {
            try
            {
                var product = await _context.Products.Include(p => p.Contents).FirstOrDefaultAsync(p => p.Id == id);
                if (product == null) return NotFound("Product not found.");

                int finalProviderId = request.ProviderId ?? product.ProviderId;

                // NEW FIX: Handle new supplier creation during updates as well
                if (request.ProviderId == 0 && !string.IsNullOrWhiteSpace(request.NewProviderName))
                {
                    var newProvider = new Provider
                    {
                        Name = request.NewProviderName,
                        CategoryId = request.CategoryId,
                        IsActive = true
                    };
                    _context.Providers.Add(newProvider);
                    await _context.SaveChangesAsync();
                    finalProviderId = newProvider.Id;
                }

                product.Name = request.Name;
                product.Price = request.Price;
                product.AvailableQuantity = request.AvailableQuantity;
                product.CategoryId = request.CategoryId;
                product.ProviderId = finalProviderId;

                var descContent = product.Contents.FirstOrDefault(c => c.ContentType == "Description");
                if (descContent != null)
                {
                    descContent.Data = request.Description ?? "";
                }
                else if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    product.Contents.Add(new ProductContent { ContentType = "Description", Data = request.Description });
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update product.", Details = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product deleted." });
        }
    }

    public class ProductDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public int CategoryId { get; set; }
        public int? ProviderId { get; set; } // Made nullable
        public string? NewProviderName { get; set; } // Added new property
        public string? Description { get; set; }
    }
}