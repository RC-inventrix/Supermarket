using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdapterService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GatewayController(IHttpClientFactory httpClientFactory)
        {
            // Injecting the factory allows us to create dynamic HTTP clients safely
            _httpClientFactory = httpClientFactory;
        }

        // 1. UNIVERSAL IMPORT
        // POST: /api/gateway/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportProducts([FromBody] GatewayRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(request.SupplierBaseUrl);

            // Dynamically call the supplier's specific catalog endpoint
            var response = await client.GetAsync(request.Endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch data from the external supplier.");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();

            // The Gateway acts as a pure pass-through relay. It fetches the external JSON 
            // and hands it cleanly back to the CoreBooking.API for database saving.
            return Content(jsonContent, "application/json");
        }

        // 2. UNIVERSAL AVAILABILITY CHECK
        // POST: /api/gateway/availability
        [HttpPost("availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] GatewayAvailabilityRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(request.SupplierBaseUrl);

            // Constructing the dynamic URL, e.g., /api/supplier/availability/MEAT-001
            var response = await client.GetAsync($"{request.Endpoint}/{request.ExternalProductId}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "External supplier is currently unreachable.");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            return Content(jsonContent, "application/json");
        }

        // 3. UNIVERSAL CHECKOUT
        // POST: /api/gateway/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> ProcessCheckout([FromBody] GatewayCheckoutRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(request.SupplierBaseUrl);

            // The payload we send to the external supplier
            var payload = new
            {
                ExternalId = request.ExternalProductId,
                Quantity = request.Quantity
            };

            var response = await client.PostAsJsonAsync(request.Endpoint, payload);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { Message = "External checkout transaction failed or stock is insufficient." });
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            return Content(jsonContent, "application/json");
        }
    }

    // ---------------------------------------------------------
    // DATA TRANSFER OBJECTS (DTOs)
    // These define the structure of the JSON that CoreBooking.API 
    // will send to this Gateway.
    // ---------------------------------------------------------

    public class GatewayRequest
    {
        public string SupplierBaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public class GatewayAvailabilityRequest : GatewayRequest
    {
        public string ExternalProductId { get; set; } = string.Empty;
    }

    public class GatewayCheckoutRequest : GatewayRequest
    {
        public string ExternalProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}