using InvoiceService.Application.Abstractions;

namespace InvoiceService.Infrastructure.Adapters;

public sealed class MockParasutAdapter : IParasutAdapter
{
    public Task<ParasutInvoiceResult> CreateInvoiceAsync(Guid orderId, CancellationToken ct)
    {
        // MVP: sahte numara + sahte pdf url
        var invoiceNo = $"PRST-{DateTime.UtcNow:yyyyMMddHHmmss}-{orderId.ToString()[..8]}";
        var pdfUrl = $"https://files.example.com/invoices/{invoiceNo}.pdf";
        return Task.FromResult(new ParasutInvoiceResult(invoiceNo, pdfUrl));
    }
}
