var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Required to wire up your GatewayController
builder.Services.AddHttpClient();  // ADDED: Enables IHttpClientFactory for dynamic external calls

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Map the controllers so HTTP requests successfully reach GatewayController
app.MapControllers();

app.Run();