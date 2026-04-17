using Microsoft.AspNetCore.Mvc;
using SpiceSupplier.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSupplier.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        // 1. The Predefined Mock Data with Initial Quantities
        private static readonly List<ExternalProduct> _spiceCatalog = new()
        {
            new ExternalProduct { ExternalId = "SPI-001", Name = "Organic Turmeric", BasePrice = 3.50m, Description = "100g jar of premium ground turmeric", AvailableQuantity = 300 },
            new ExternalProduct { ExternalId = "SPI-002", Name = "Smoked Paprika", BasePrice = 4.25m, Description = "75g tin of Spanish smoked paprika", AvailableQuantity = 150 },
            new ExternalProduct { ExternalId = "SPI-003", Name = "Whole Black Peppercorns", BasePrice = 5.99m, Description = "200g refill bag of black pepper", AvailableQuantity = 400 }
        };

        // 2. Endpoint: Import Products
        // GET: /api/supplier/catalog
        [HttpGet("catalog")]
        public IActionResult GetCatalog()
        {
            return Ok(_spiceCatalog);
        }

        // 3. Endpoint: Check Availability
        // GET: /api/supplier/availability/{id}
        [HttpGet("availability/{id}")]
        public IActionResult CheckAvailability(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Ok(new { ExternalId = "SAMPLE-ID", AvailableStock = 99 });
            }
            var product = _spiceCatalog.FirstOrDefault(p => p.ExternalId == id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found in Spice Supplier system." });
            }

            return Ok(new { ExternalId = id, AvailableStock = product.AvailableQuantity });
        }

        // 4. Endpoint: Checkout and Booking
        // POST: /api/supplier/checkout
        [HttpPost("checkout")]
        public IActionResult ProcessCheckout([FromBody] CheckoutRequest request)
        {
            var product = _spiceCatalog.FirstOrDefault(p => p.ExternalId == request.ExternalId);

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
            string bookingRef = $"SPI-CONF-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

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