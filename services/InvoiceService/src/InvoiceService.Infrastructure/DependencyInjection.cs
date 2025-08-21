using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Adapters;
using InvoiceService.Infrastructure.Messaging;
using InvoiceService.Infrastructure.Outbox;
using InvoiceService.Infrastructure.Persistence;
using InvoiceService.Infrastructure.Repositories;

namespace InvoiceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("InvoiceDb")
                   ?? "Host=invoice-db;Database=invoicedb;Username=postgres;Password=postgres";

        services.AddDbContext<InvoiceDbContext>(opt => opt.UseNpgsql(conn));

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IParasutAdapter, MockParasutAdapter>();

        services.Configure<RabbitMqOptions>(config.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        services.AddHostedService<OrderCreatedConsumer>();   // consume order.created
        services.AddHostedService<OutboxPublisherWorker>();  // publish order.invoiced

        return services;
    }
}
