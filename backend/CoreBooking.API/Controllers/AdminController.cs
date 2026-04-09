using CoreBooking.Domain.Adapters;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost("import-products/{providerId}/{categoryId}")]
        public async Task<IActionResult> ImportProducts(int providerId, int categoryId)
        {
            // 1. Find the provider in our DB to know which adapter to use
            var provider = await _context.Providers.FindAsync(providerId);
            if (provider == null) return NotFound("Provider not found.");

            // 2. Instantiate the correct adapter using the factory
            var adapter = _adapterFactory(provider.AdapterKey);

            // 3. Fetch and translate the data
            var importedProducts = await adapter.GetProductsAsync(providerId, categoryId);

            // 4. Save to our Core SQL Database
            _context.Products.AddRange(importedProducts);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully imported {importedProducts.Count()} products." });
        }
    }
}