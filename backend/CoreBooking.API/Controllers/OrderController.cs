using CoreBooking.API.Services;
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
        private readonly IntegrationGatewayClient _gatewayClient;

        public OrderController(CoreBookingDbContext context, IntegrationGatewayClient gatewayClient)
        {
            _context = context;
            _gatewayClient = gatewayClient;
        }

        // --- NEW FIX: Get All Orders for the UI ---
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .AsQueryable();

                // Search by Confirmation Code or Order ID
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(o => (o.ExternalBookingReference != null && o.ExternalBookingReference.Contains(search))
                                          || o.Id.ToString() == search);
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var orders = await query
                    .OrderByDescending(o => o.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        id = o.Id,
                        customerName = "admin", // Hardcoded per requirement
                        totalAmount = o.TotalAmount,
                        status = "Confirmed",   // Hardcoded per requirement (Status = 1)
                        confirmationCode = o.ExternalBookingReference ?? "N/A",
                        createdAt = o.CreatedAt,
                        items = o.Items.Select(i => new
                        {
                            id = i.Id,
                            productName = i.Product != null ? i.Product.Name : "Unknown Product",
                            quantity = i.Quantity,
                            unitPrice = i.PriceAtTimeOfBooking,
                            totalPrice = i.Quantity * i.PriceAtTimeOfBooking
                        })
                    })
                    .ToListAsync();

                return Ok(new { items = orders, totalCount, totalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch orders.", Details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Order not found.");

            var result = new
            {
                id = order.Id,
                customerName = "admin",
                totalAmount = order.TotalAmount,
                status = "Confirmed",
                confirmationCode = order.ExternalBookingReference ?? "N/A",
                createdAt = order.CreatedAt,
                items = order.Items.Select(i => new
                {
                    id = i.Id,
                    productName = i.Product != null ? i.Product.Name : "Unknown Product",
                    quantity = i.Quantity,
                    unitPrice = i.PriceAtTimeOfBooking,
                    totalPrice = i.Quantity * i.PriceAtTimeOfBooking
                })
            };

            return Ok(result);
        }

        // Bulk Checkout for Cart
        [HttpPost("checkout/cart")]
        public async Task<IActionResult> CheckoutCart([FromBody] CartCheckoutRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            if (cart == null) return NotFound("Cart not found.");

            var itemsToCheckout = await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.Provider)
                .Where(ci => request.CartItemIds.Contains(ci.Id) && ci.CartId == cart.Id)
                .ToListAsync();

            if (!itemsToCheckout.Any()) return BadRequest("No items selected for checkout.");

            decimal totalAmount = 0;
            var externalRefs = new List<string>();
            var orderItems = new List<OrderItem>();

            foreach (var item in itemsToCheckout)
            {
                var product = item.Product;
                if (product == null || product.Provider == null) continue;

                // Dynamic Availability Check
                int availableStock = await _gatewayClient.CheckAvailabilityAsync(product.Provider, product.ExternalProductId);
                if (availableStock < item.Quantity)
                {
                    return BadRequest($"Insufficient stock for {product.Name}. Only {availableStock} left at supplier.");
                }

                // Dynamic Order Placement
                string bookingRef = await _gatewayClient.PlaceOrderAsync(product.Provider, product.ExternalProductId, item.Quantity);
                externalRefs.Add(bookingRef);

                // Reduce local stock cache to stay synced
                product.AvailableQuantity -= item.Quantity;

                totalAmount += (item.UnitPrice * item.Quantity);

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    PriceAtTimeOfBooking = item.UnitPrice
                });
            }

            var order = new Order
            {
                UserId = request.UserId,
                TotalAmount = totalAmount,
                Status = "Confirmed",
                ExternalBookingReference = string.Join(", ", externalRefs),
                Items = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            foreach (var item in itemsToCheckout)
            {
                cart.Items.Remove(item);
                _context.CartItems.Remove(item);
            }
            cart.TotalPrice = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order Confirmed! 1", Status = 1, ExternalReference = order.ExternalBookingReference });
        }
    }

    public class CartCheckoutRequest
    {
        public int UserId { get; set; }
        public List<int> CartItemIds { get; set; } = new List<int>();
    }
}