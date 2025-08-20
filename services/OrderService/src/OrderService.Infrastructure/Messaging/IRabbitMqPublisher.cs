namespace OrderService.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    void Publish(string routingKey, ReadOnlyMemory<byte> body, IDictionary<string, object>? headers = null);
}
