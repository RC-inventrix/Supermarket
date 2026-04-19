using CoreBooking.Domain.Entities;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;

        public CartController(CoreBookingDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserCart(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Provider) // Fetches the Supplier!
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Ok(new { UserId = userId, TotalPrice = 0, Items = new List<object>() });
            }

            // THE FIX: Map to DTO to prevent JSON cycle crashes and provide a clean payload for React
            var result = new
            {
                Id = cart.Id,
                UserId = cart.UserId,
                TotalPrice = cart.TotalPrice,
                Items = cart.Items.Select(i => new
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product?.Name ?? "Unknown Product",
                    Price = i.UnitPrice,
                    Quantity = i.Quantity,
                    Supplier = i.Product?.Provider?.Name ?? "Unknown Supplier"
                })
            };

            return Ok(result);
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] CartItemRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Product not found.");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, TotalPrice = 0 };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = product.Price;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price
                };
                _context.CartItems.Add(newItem);
                cart.Items.Add(newItem);
            }

            cart.TotalPrice = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Added to cart." });
        }

        [HttpDelete("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int cartItemId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null) return NotFound("Item not found in this user's cart.");

            _context.CartItems.Remove(item);
            cart.Items.Remove(item);

            cart.TotalPrice = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Item removed from cart." });
        }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}