using CoreBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CoreBooking.API.Services;
using CoreBooking.Domain.Adapters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the DbContext with the connection string from appsettings.json
builder.Services.AddDbContext<CoreBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Register the typed HTTP Clients pointing to your running external microservices
builder.Services.AddHttpClient<MeatSupplierAdapter>(client =>
    client.BaseAddress = new Uri("https://localhost:32771")); 

builder.Services.AddHttpClient<VeggieSupplierAdapter>(client =>
    client.BaseAddress = new Uri("https://localhost:32773"));

builder.Services.AddHttpClient<SpiceSupplierAdapter>(client =>
    client.BaseAddress = new Uri("https://localhost:32769"));

// 2. Register the Adapter Factory 
// This reads the AdapterKey from the database and resolves the correct C# class dynamically!
builder.Services.AddTransient<Func<string, IExternalProviderAdapter>>(serviceProvider => key =>
{
return key switch
{
    AdapterKeys.Meat => serviceProvider.GetRequiredService<MeatSupplierAdapter>(),
    AdapterKeys.Veggie => serviceProvider.GetRequiredService<VeggieSupplierAdapter>(),
    AdapterKeys.Spice => serviceProvider.GetRequiredService<SpiceSupplierAdapter>(),
    _ => throw new KeyNotFoundException($"Adapter {key} not found.")
};
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Optional: If you want Swagger UI (the visual web page) in .NET 10, 
    // you would typically add app.UseSwaggerUI() here after installing the Swashbuckle package.
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();