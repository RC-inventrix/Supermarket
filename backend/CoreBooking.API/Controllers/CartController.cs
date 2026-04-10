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
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Return an empty logical cart if the user hasn't added anything yet
                return Ok(new { UserId = userId, Items = new List<object>() });
            }

            return Ok(cart);
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] CartItemRequest request)
        {
            // 1. Ensure the product actually exists
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Product not found.");

            // 2. Find existing cart, or create a new one
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Save to generate the Cart Id
            }

            // 3. Check if product is already in the cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Added to cart." });
        }

        [HttpDelete("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            if (item == null) return NotFound("Item not found in this user's cart.");

            _context.CartItems.Remove(item);
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