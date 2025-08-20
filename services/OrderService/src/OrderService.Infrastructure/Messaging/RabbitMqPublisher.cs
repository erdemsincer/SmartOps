using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly AsyncRetryPolicy _retry;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port     = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _options.Exchange,
                                 type: ExchangeType.Topic,
                                 durable: _options.DurableExchange);

        _retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1)
            }, (ex, ts, i, ctx) =>
            {
                _logger.LogWarning(ex, "RabbitMQ publish retry {Attempt}", i);
            });
    }

    public void Publish(string routingKey, ReadOnlyMemory<byte> body, IDictionary<string, object>? headers = null)
    {
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        if (headers is not null) props.Headers = headers;

        // Retry senkron publish için küçük bir yardımcı
        _retry.ExecuteAsync(async () =>
        {
            _channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: routingKey,
                basicProperties: props,
                body: body);
            await Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
