using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvoiceService.Infrastructure.Messaging;
using InvoiceService.Infrastructure.Persistence;

namespace InvoiceService.Infrastructure.Outbox;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(IServiceScopeFactory scopeFactory, IRabbitMqPublisher publisher, ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();

                var batch = await db.OutboxMessages
                    .Where(x => x.ProcessedAtUtc == null)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                {
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                foreach (var msg in batch)
                {
                    try
                    {
                        var routingKey = TypeToRoutingKey(msg.Type); // order.invoiced
                        _publisher.Publish(routingKey, Encoding.UTF8.GetBytes(msg.Payload));
                        msg.ProcessedAtUtc = DateTime.UtcNow;
                        msg.Attempt += 1; msg.Error = null;
                    }
                    catch (Exception ex)
                    {
                        msg.Attempt += 1; msg.Error = ex.Message;
                        _logger.LogError(ex, "Outbox publish failed {Id}", msg.Id);
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested) {}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox loop error");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private static string TypeToRoutingKey(string type)
        => type.EndsWith("OrderInvoicedIntegrationEvent", StringComparison.OrdinalIgnoreCase)
            ? "order.invoiced"
            : "unknown.event";
}
