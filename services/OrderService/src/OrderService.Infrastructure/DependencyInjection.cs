using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Serialization;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("OrderDb")
                   ?? "Host=order-db;Database=orderdb;Username=postgres;Password=postgres";

        services.AddDbContext<OrderDbContext>(opt => opt.UseNpgsql(conn));

        // Repos
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Outbox writer
        services.AddScoped<IOutboxWriter, Outbox.OutboxWriter>();

        // RabbitMQ (fix)
        services.Configure<RabbitMqOptions>(
            options => config.GetSection(RabbitMqOptions.SectionName).Bind(options)
        );
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // JSON
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

        // Background worker
        services.AddHostedService<OutboxPublisherWorker>();

        return services;
    }
}
