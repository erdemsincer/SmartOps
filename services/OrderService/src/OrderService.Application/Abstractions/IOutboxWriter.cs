using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Abstractions;

public interface IOutboxWriter
{
    Task WriteAsync(string type, string payloadJson, DateTime occurredAtUtc, CancellationToken ct = default);
}
