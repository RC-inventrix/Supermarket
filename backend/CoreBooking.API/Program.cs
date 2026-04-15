using CoreBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CoreBooking.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register the DbContext
builder.Services.AddDbContext<CoreBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REPLACED: Old Adapter Factory Logic ---
// We now register a single HTTP Client pointing to the AdapterService.API Docker container
var gatewayUrl = builder.Configuration["AdapterServiceUrl"] ?? "http://adapterservice.api:8080";
builder.Services.AddHttpClient<IntegrationGatewayClient>(client =>
    client.BaseAddress = new Uri(gatewayUrl));
// -------------------------------------------

builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();