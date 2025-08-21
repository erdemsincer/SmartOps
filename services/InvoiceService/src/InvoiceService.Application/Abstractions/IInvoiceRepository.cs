using InvoiceService.Domain.Entities;

namespace InvoiceService.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken ct);
    Task AddAsync(Invoice invoice, CancellationToken ct);
}
