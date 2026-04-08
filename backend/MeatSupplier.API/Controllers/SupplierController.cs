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
        // Note: Because this is 'static', the inventory will persist in memory 
        // as long as the Docker container/Visual Studio project is running.
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
            // Returns the list of products including their current quantities
            return Ok(_meatCatalog);
        }

        // 3. Endpoint: Check Availability
        // GET: /api/supplier/availability/{id}
        [HttpGet("availability/{id}")]
        public IActionResult CheckAvailability(string id)
        {
            var product = _meatCatalog.FirstOrDefault(p => p.ExternalId == id);
            if (product == null)
            {
                return NotFound(new { Message = "Product not found in Meat Supplier system." });
            }

            // Return the actual live stock number from our in-memory list
            return Ok(new { ExternalId = id, AvailableStock = product.AvailableQuantity });
        }

        // 4. Endpoint: Checkout and Booking
        // POST: /api/supplier/checkout
        [HttpPost("checkout")]
        public IActionResult ProcessCheckout([FromBody] CheckoutRequest request)
        {
            var product = _meatCatalog.FirstOrDefault(p => p.ExternalId == request.ExternalId);

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
            string bookingRef = $"MEAT-CONF-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

            return Ok(new CheckoutResponse(true, bookingRef, $"Order successful. {request.Quantity} units deducted. New stock: {product.AvailableQuantity}"));
        }
    }
}