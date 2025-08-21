using InvoiceService.Application.Abstractions;
using InvoiceService.Infrastructure.Persistence;

namespace InvoiceService.Infrastructure.Outbox;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly InvoiceDbContext _db;
    public OutboxWriter(InvoiceDbContext db) => _db = db;

    public async Task WriteAsync(string type, string payloadJson, DateTime occurredAtUtc, CancellationToken ct)
    {
        var msg = new OutboxMessage
        {
            Type = type,
            Payload = payloadJson,
            OccurredAtUtc = occurredAtUtc
        };
        await _db.OutboxMessages.AddAsync(msg, ct);
        await _db.SaveChangesAsync(ct);
    }
}
