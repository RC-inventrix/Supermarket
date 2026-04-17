using CoreBooking.API.Services;
using CoreBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly CoreBookingDbContext _context;
        private readonly IntegrationGatewayClient _gatewayClient;

        public SuppliersController(CoreBookingDbContext context, IntegrationGatewayClient gatewayClient)
        {
            _context = context;
            _gatewayClient = gatewayClient;
        }

        // GET: /api/suppliers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var providers = await _context.Providers.ToListAsync();

                var result = providers.Select(p =>
                {
                    object? parsedMapping = null;

                    // SAFE PARSING: If an old database row has bad JSON, it won't crash the whole API
                    if (!string.IsNullOrWhiteSpace(p.MappingConfigJson))
                    {
                        try
                        {
                            parsedMapping = JsonSerializer.Deserialize<object>(p.MappingConfigJson);
                        }
                        catch
                        {
                            // Silently ignore invalid JSON from old records
                        }
                    }

                    return new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryId = p.CategoryId,
                        SupplierBaseUrl = p.SupplierBaseUrl,
                        CatalogEndpoint = p.CatalogEndpoint,
                        AvailabilityEndpoint = p.AvailabilityEndpoint,
                        CheckoutEndpoint = p.CheckoutEndpoint,
                        IsActive = true, // Defaulting to true so the React UI doesn't break
                        CreatedAt = DateTime.UtcNow, // Defaulting to satisfy React UI
                        MappingConfig = parsedMapping
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Returns a clear error message instead of failing silently
                return StatusCode(500, new { Message = "Failed to load suppliers from database.", Details = ex.Message });
            }
        }

        // POST: /api/suppliers/fetch-sample
        [HttpPost("fetch-sample")]
        public async Task<IActionResult> FetchSample([FromBody] FetchSampleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BaseUrl) || string.IsNullOrWhiteSpace(request.Endpoint))
            {
                return BadRequest("BaseUrl and Endpoint are required.");
            }

            try
            {
                var jsonString = await _gatewayClient.FetchSampleJsonAsync(request.BaseUrl, request.Endpoint);
                return Content(jsonString, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }

    public class FetchSampleRequest
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}