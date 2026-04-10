using CoreBooking.Domain.Adapters;
using CoreBooking.Domain.Entities;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;
        private readonly Func<string, IExternalProviderAdapter> _adapterFactory;

        public OrderController(CoreBookingDbContext context, Func<string, IExternalProviderAdapter> adapterFactory)
        {
            _context = context;
            _adapterFactory = adapterFactory;
        }

        // --- EXISTING FUNCTIONALITY (Untouched) ---
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(int userId, int productId, int quantity)
        {
            var product = await _context.Products
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound("Product not found locally.");

            var adapter = _adapterFactory(product.Provider.AdapterKey);

            int availableStock = await adapter.CheckAvailabilityAsync(product.ExternalProductId);
            if (availableStock < quantity) return BadRequest("Insufficient external stock.");

            string bookingRef = await adapter.PlaceOrderAsync(product.ExternalProductId, quantity);

            var order = new Order
            {
                UserId = userId,
                TotalAmount = product.Price * quantity,
                Status = "Confirmed",
                ExternalBookingReference = bookingRef
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order Confirmed!", ExternalReference = bookingRef });
        }

        // --- ADDED CHANGES BEGIN: View & Update Orders ---

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found.");

            return Ok(order);
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            order.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Order status updated to '{order.Status}'.", OrderId = order.Id });
        }

        // --- ADDED CHANGES END ---
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}