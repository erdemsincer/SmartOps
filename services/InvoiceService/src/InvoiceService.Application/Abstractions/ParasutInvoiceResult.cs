namespace InvoiceService.Application.Abstractions;

public sealed record ParasutInvoiceResult(string InvoiceNo, string PdfUrl);

public interface IParasutAdapter
{
    Task<ParasutInvoiceResult> CreateInvoiceAsync(Guid orderId, CancellationToken ct);
}
