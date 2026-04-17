using MeatSupplier.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeatSupplier.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        // 1. The Predefined Mock Data with Initial Quantities
        private static readonly List<ExternalProduct> _meatCatalog = new()
        {
            new ExternalProduct { ExternalId = "MEAT-001", Name = "Premium Ribeye Steak", BasePrice = 25.99m, Description = "16oz Prime Cut Beef", AvailableQuantity = 50 },
            new ExternalProduct { ExternalId = "MEAT-002", Name = "Fresh Chicken Breast", BasePrice = 9.50m, Description = "1kg Boneless Skinless", AvailableQuantity = 120 },
            new ExternalProduct { ExternalId = "MEAT-003", Name = "Pork Chops", BasePrice = 14.00m, Description = "Thick Cut Bone-in Pork", AvailableQuantity = 30 }
        };

        // 2. Endpoint: Import Products
        // GET: /api/supplier/catalog
        [HttpGet("catalog")]
        public IActionResult GetCatalog()
        {
            return Ok(_meatCatalog);
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
            var product = _meatCatalog.FirstOrDefault(p => p.ExternalId == id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found in Meat Supplier system." });
            }

            return Ok(new { ExternalId = id, AvailableStock = product.AvailableQuantity });
        }

        // 4. Endpoint: Checkout and Booking (REAL)
        // POST: /api/supplier/checkout
        [HttpPost("checkout")]
        public IActionResult ProcessCheckout([FromBody] CheckoutRequest request)
        {
            var product = _meatCatalog.FirstOrDefault(p => p.ExternalId == request.ExternalId);

            if (product == null)
            {
                return BadRequest(new CheckoutResponse(false, "", "Invalid Product ID"));
            }

            if (product.AvailableQuantity < request.Quantity)
            {
                return BadRequest(new CheckoutResponse(false, "", $"Insufficient stock. Only {product.AvailableQuantity} available."));
            }

            product.AvailableQuantity -= request.Quantity;
            string bookingRef = $"MEAT-CONF-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

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