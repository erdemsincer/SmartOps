using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions;   // IOutboxWriter
using OrderService.Infrastructure.Outbox;     // OutboxWriter
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("OrderDb")
                   ?? "Host=order-db;Database=orderdb;Username=postgres;Password=postgres";

        services.AddDbContext<OrderDbContext>(opt => opt.UseNpgsql(conn));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        return services;
    }
}
 