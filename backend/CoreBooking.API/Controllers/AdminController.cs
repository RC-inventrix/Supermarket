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

        [HttpPut("sync-availability/{productId}")]
        public async Task<IActionResult> SyncProductAvailability(int productId)
        {
            var product = await _context.Products.Include(p => p.Provider).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || product.Provider == null) return NotFound("Product or Provider not found.");

            product.AvailableQuantity = await _gatewayClient.CheckAvailabilityAsync(product.Provider, product.ExternalProductId);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Successfully synced availability for '{product.Name}'.", LiveStock = product.AvailableQuantity });
        }

        // --- NEW FIX: Get Dynamic Dashboard Statistics ---
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeSuppliers = await _context.Providers.CountAsync(p => p.IsActive);
            var totalOrders = await _context.Orders.CountAsync();

            return Ok(new
            {
                TotalProducts = totalProducts,
                ActiveSuppliers = activeSuppliers,
                TotalOrders = totalOrders
            });
        }

        // --- NEW FIX: Verify Live System Connection Health ---
        [HttpGet("system-status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            // 1. Check if the SQL Server Database responds
            bool backendConnected = await _context.Database.CanConnectAsync();

            // 2. Check if the Adapter Gateway Container responds
            bool gatewayConnected = await _gatewayClient.PingAsync();

            return Ok(new
            {
                BackendConnected = backendConnected,
                GatewayConnected = gatewayConnected
            });
        }
    }
}