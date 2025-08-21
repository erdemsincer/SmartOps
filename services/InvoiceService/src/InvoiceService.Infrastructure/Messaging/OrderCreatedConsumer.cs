using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartOps.Contracts.Orders;
using InvoiceService.Application.UseCases;

namespace InvoiceService.Infrastructure.Messaging;

public sealed class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _opt;
    private IConnection? _conn;
    private IModel? _ch;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _opt = options.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var f = new ConnectionFactory { HostName=_opt.HostName, Port=_opt.Port, UserName=_opt.UserName, Password=_opt.Password, VirtualHost=_opt.VirtualHost, DispatchConsumersAsync = true };
        _conn = f.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: _opt.DurableExchange);
        _ch.QueueDeclare(queue: _opt.Queue, durable: true, exclusive: false, autoDelete: false);
        _ch.QueueBind(queue: _opt.Queue, exchange: _opt.Exchange, routingKey: "order.created");

        var consumer = new AsyncEventingBasicConsumer(_ch);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ev = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(json);
                if (ev is null) { _ch!.BasicNack(ea.DeliveryTag, false, false); return; }

                using var scope = _scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<ICreateInvoiceForOrder>();
                await useCase.HandleAsync(ev, stoppingToken);

                _ch!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "consume error");
                _ch!.BasicNack(ea.DeliveryTag, false, true); // requeue
            }
        };

        _ch.BasicQos(0, 10, false);
        _ch.BasicConsume(queue: _opt.Queue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        try { _ch?.Close(); } catch { }
        try { _conn?.Close(); } catch { }
        _ch?.Dispose(); _conn?.Dispose();
        base.Dispose();
    }
}
