namespace SmartOps.Contracts.Orders;

public sealed record OrderItemContract(string Sku, string Name, int Quantity, decimal Price);

public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    string Channel,
    string ChannelOrderId,
    decimal TotalAmount,
    string CustomerName,
    string AddressJson,
    IReadOnlyList<OrderItemContract> Items,
    DateTime OccurredAtUtc
);
