using Microsoft.EntityFrameworkCore;
using InvoiceService.Application.Abstractions;
using InvoiceService.Domain.Entities;
using InvoiceService.Infrastructure.Persistence;

namespace InvoiceService.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDbContext _db;
    public InvoiceRepository(InvoiceDbContext db) => _db = db;

    public Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken ct) =>
        _db.Invoices.AnyAsync(x => x.OrderId == orderId, ct);

    public async Task AddAsync(Invoice invoice, CancellationToken ct)
    {
        await _db.Invoices.AddAsync(invoice, ct);
        await _db.SaveChangesAsync(ct);
    }
}
