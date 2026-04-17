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

        // Removed the Duplicate ImportProducts endpoint! 
        // SuppliersController handles all imports now via POST /api/suppliers/{id}/import.

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