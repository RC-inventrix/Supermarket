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

        // --- PRODUCT MANAGEMENT ---

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Provider)
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Attributes)
                .Include(p => p.Contents)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound("Product not found.");
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updateData)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = updateData.Name;
            product.Price = updateData.Price;
            product.AvailableQuantity = updateData.AvailableQuantity;

            await _context.SaveChangesAsync();
            return Ok(product);
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

        // --- PRODUCT ATTRIBUTES ---

        [HttpPost("{productId}/attributes")]
        public async Task<IActionResult> AddAttribute(int productId, [FromBody] ProductAttribute attribute)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("Product not found.");

            attribute.ProductId = productId;
            _context.ProductAttributes.Add(attribute);
            await _context.SaveChangesAsync();

            return Ok(attribute);
        }

        [HttpDelete("{productId}/attributes/{attributeId}")]
        public async Task<IActionResult> DeleteAttribute(int productId, int attributeId)
        {
            var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(a => a.Id == attributeId && a.ProductId == productId);
            if (attribute == null) return NotFound();

            _context.ProductAttributes.Remove(attribute);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Attribute deleted." });
        }

        // --- PRODUCT CONTENT ---

        [HttpPost("{productId}/contents")]
        public async Task<IActionResult> AddContent(int productId, [FromBody] ProductContent content)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("Product not found.");

            content.ProductId = productId;
            _context.ProductContents.Add(content);
            await _context.SaveChangesAsync();

            return Ok(content);
        }

        [HttpDelete("{productId}/contents/{contentId}")]
        public async Task<IActionResult> DeleteContent(int productId, int contentId)
        {
            var content = await _context.ProductContents.FirstOrDefaultAsync(c => c.Id == contentId && c.ProductId == productId);
            if (content == null) return NotFound();

            _context.ProductContents.Remove(content);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Content deleted." });
        }
    }
}