using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;
using OrderService.Application;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    // Sadece gerekiyorsa aktif tut:
    .AddNewtonsoftJson();

builder.Services.AddFluentValidationAutoValidation();
// builder.Services.AddValidatorsFromAssemblyContaining<YourAnyValidatorType>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// (opsiyonel) ProblemDetails + global exception
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();       // istek logları
app.UseExceptionHandler();            // ProblemDetails ile 5xx/4xx

// Auto-migrate (dev)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "DB migrate failed");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "order-service" }));
app.MapGet("/ready", async (OrderDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect ? Results.Ok(new { db = "up" }) : Results.Problem("db down", statusCode: 503);
});

app.Run();
