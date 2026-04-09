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

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(int userId, int productId, int quantity)
        {
            // 1. Get the product and its provider from our local DB
            var product = await _context.Products
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound("Product not found locally.");

            var adapter = _adapterFactory(product.Provider.AdapterKey);

            // 2. Check real-time availability with the external system
            int availableStock = await adapter.CheckAvailabilityAsync(product.ExternalProductId);
            if (availableStock < quantity) return BadRequest("Insufficient external stock.");

            // 3. Perform the external checkout
            string bookingRef = await adapter.PlaceOrderAsync(product.ExternalProductId, quantity);

            // 4. Save the confirmed order in our local database
            var order = new Order
            {
                UserId = userId,
                TotalAmount = product.Price * quantity,
                Status = "Confirmed",
                ExternalBookingReference = bookingRef // Storing the required confirmation code
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order Confirmed!", ExternalReference = bookingRef });
        }
    }
}