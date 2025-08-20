using System.Text.Json;
using OrderService.Application.Abstractions;
using OrderService.Application.Incoming;
using OrderService.Domain.Entities;
using SmartOps.Contracts.Orders;

namespace OrderService.Application.UseCases;

public interface ICreateOrderFromTrendyol
{
    Task<Guid> HandleAsync(TrendyolWebhookDto dto, CancellationToken ct);
}

public sealed class CreateOrderFromTrendyol : ICreateOrderFromTrendyol
{
    private readonly IOrderRepository _orders;
    private readonly IOutboxWriter _outbox;

    public CreateOrderFromTrendyol(IOrderRepository orders, IOutboxWriter outbox)
    {
        _orders = orders;
        _outbox = outbox;
    }

    public async Task<Guid> HandleAsync(TrendyolWebhookDto dto, CancellationToken ct)
    {
        if (await _orders.ExistsByChannelOrderIdAsync(dto.ChannelOrderId, ct))
            return Guid.Empty; // idempotency: zaten var

        var order = Order.Create(
            channel: "trendyol",
            channelOrderId: dto.ChannelOrderId,
            totalAmount: dto.TotalAmount,
            customerName: dto.CustomerName,
            addressJson: dto.AddressJson);

        foreach (var i in dto.Items)
            order.AddItem(i.Sku, i.Name, i.Quantity, i.Price);

        await _orders.AddAsync(order, ct);

        // Outbox event
        var ev = new OrderCreatedIntegrationEvent(
            order.Id,
            order.Channel,
            order.ChannelOrderId,
            order.TotalAmount,
            order.CustomerName,
            order.AddressJson,
            order.Items.Select(x => new OrderItemContract(x.Sku, x.Name, x.Quantity, x.Price)).ToList(),
            DateTime.UtcNow
        );

        var payload = JsonSerializer.Serialize(ev);
        await _outbox.WriteAsync(
            type: typeof(OrderCreatedIntegrationEvent).FullName!,
            payloadJson: payload,
            occurredAtUtc: ev.OccurredAtUtc,
            ct: ct);

        return order.Id;
    }
}
