using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using VeggieSupplier.API.Models;

namespace VeggieSupplier.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        // 1. The Predefined Mock Data with Initial Quantities
        private static readonly List<ExternalProduct> _veggieCatalog = new()
        {
            new ExternalProduct { ExternalId = "VEG-001", Name = "Organic Carrots", BasePrice = 2.99m, Description = "1kg bag of farm-fresh carrots", AvailableQuantity = 200 },
            new ExternalProduct { ExternalId = "VEG-002", Name = "Fresh Spinach", BasePrice = 4.50m, Description = "500g baby spinach leaves", AvailableQuantity = 80 },
            new ExternalProduct { ExternalId = "VEG-003", Name = "Bell Peppers Mix", BasePrice = 5.00m, Description = "3-pack of red, yellow, and green peppers", AvailableQuantity = 150 }
        };

        // 2. Endpoint: Import Products
        // GET: /api/supplier/catalog
        [HttpGet("catalog")]
        public IActionResult GetCatalog()
        {
            return Ok(_veggieCatalog);
        }

        // 3. Endpoint: Check Availability
        // GET: /api/supplier/availability/{id?}  <-- Notice the '?' making the ID optional
        [HttpGet("availability/{id?}")]
        public IActionResult CheckAvailability(string? id)
        {
            // NEW: If no ID is provided, assume the React UI is just asking for a sample JSON structure!
            if (string.IsNullOrEmpty(id))
            {
                return Ok(new { ExternalId = "SAMPLE-ID", AvailableStock = 99 });
            }

            // REAL LOGIC:
            var product = _veggieCatalog.FirstOrDefault(p => p.ExternalId == id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found in Veggie Supplier system." });
            }

            return Ok(new { ExternalId = id, AvailableStock = product.AvailableQuantity });
        }

        // 4. Endpoint: Checkout and Booking
        // POST: /api/supplier/checkout
        [HttpPost("checkout")]
        public IActionResult ProcessCheckout([FromBody] CheckoutRequest request)
        {
            var product = _veggieCatalog.FirstOrDefault(p => p.ExternalId == request.ExternalId);

            // Validate the product exists
            if (product == null)
            {
                return BadRequest(new CheckoutResponse(false, "", "Invalid Product ID"));
            }

            // Validate that we have enough stock to fulfill the order
            if (product.AvailableQuantity < request.Quantity)
            {
                return BadRequest(new CheckoutResponse(false, "", $"Insufficient stock. Only {product.AvailableQuantity} available."));
            }

            // DEDUCT THE INVENTORY
            product.AvailableQuantity -= request.Quantity;

            // Generate the confirmation reference
            string bookingRef = $"VEG-CONF-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

            return Ok(new CheckoutResponse(true, bookingRef, $"Order successful. {request.Quantity} units deducted. New stock: {product.AvailableQuantity}"));
        }

        // 5. Endpoint: Checkout Sample (NEW FOR UI MAPPER)
        // GET: /api/supplier/checkout
        [HttpGet("checkout")]
        public IActionResult GetCheckoutSample()
        {
            // Returns a dummy response so the React UI can map the confirmation path
            return Ok(new CheckoutResponse(true, "SAMPLE-CONF-123456", "Sample order successful."));
        }
    }
}