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

// 1. Configure the CORS Policy
// 1. Configure the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // Added port 3001 right here!
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- OPTIMIZED MIDDLEWARE PIPELINE ---
app.UseHttpsRedirection();

app.UseRouting();             // 1st: Identify the route
app.UseCors("AllowReactApp"); // 2nd: Apply CORS rules for that route
app.UseAuthorization();       // 3rd: Apply Authorization

app.MapControllers();         // 4th: Execute the controller
// -------------------------------------

app.Run();