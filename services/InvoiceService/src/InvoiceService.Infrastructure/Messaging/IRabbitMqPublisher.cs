using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace InvoiceService.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    void Publish(string routingKey, ReadOnlyMemory<byte> body);
}

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMQ.Client.IModel _ch;
    private readonly RabbitMQ.Client.IConnection _conn;
    private readonly RabbitMqOptions _opt;
    private readonly AsyncRetryPolicy _retry;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _opt = options.Value;

        var f = new ConnectionFactory
        {
            HostName = _opt.HostName,
            Port = _opt.Port,
            UserName = _opt.UserName,
            Password = _opt.Password,
            VirtualHost = _opt.VirtualHost,
            DispatchConsumersAsync = true
        };

        _conn = f.CreateConnection();
        _ch = _conn.CreateModel();
        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: _opt.DurableExchange);

        _retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1)
                },
                (ex, ts, i, ctx) => _logger.LogWarning(ex, "publish retry {i}", i)
            );
    }

    public void Publish(string routingKey, ReadOnlyMemory<byte> body)
    {
        var props = _ch.CreateBasicProperties();
        props.Persistent = true;

        _retry.ExecuteAsync(async () =>
        {
            _ch.BasicPublish(_opt.Exchange, routingKey, props, body);
            await Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        try { _ch?.Close(); } catch { }
        try { _conn?.Close(); } catch { }
        _ch?.Dispose();
        _conn?.Dispose();
    }
}
