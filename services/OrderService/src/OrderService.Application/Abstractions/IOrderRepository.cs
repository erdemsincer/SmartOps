using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions;

public interface IOrderRepository
{
   public Task<bool> ExistsByChannelOrderIdAsync(string channelOrderId, CancellationToken ct);
  public Task AddAsync(Order order, CancellationToken ct);
}
