using CoreBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the DbContext with the connection string from appsettings.json
builder.Services.AddDbContext<CoreBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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