using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.Abstractions;
using OrderService.Infrastructure.Persistence; // <-- tek adet

namespace OrderService.Infrastructure.Outbox
{
    public sealed class OutboxWriter : IOutboxWriter
    {
        private readonly OrderDbContext _db;
        public OutboxWriter(OrderDbContext db) => _db = db;

        public async Task WriteAsync(string type, string payloadJson, DateTime occurredAtUtc, CancellationToken ct = default)
        {
            var msg = new OutboxMessage
            {
                Type = type,
                Payload = payloadJson,
                OccurredAtUtc = occurredAtUtc,
                Attempt = 0
            };

            await _db.OutboxMessages.AddAsync(msg, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
