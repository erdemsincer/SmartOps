namespace InvoiceService.Application.Abstractions;

public interface IOutboxWriter
{
    Task WriteAsync(string type, string payloadJson, DateTime occurredAtUtc, CancellationToken ct);
}
