using System.Text.Json;
using InvoiceService.Application.Abstractions;
using InvoiceService.Domain.Entities;
using SmartOps.Contracts.Orders;

namespace InvoiceService.Application.UseCases;

public interface ICreateInvoiceForOrder
{
   public Task HandleAsync(OrderCreatedIntegrationEvent ev, CancellationToken ct);
}

public sealed class CreateInvoiceForOrder : ICreateInvoiceForOrder
{
    private readonly IInvoiceRepository _repo;
    private readonly IParasutAdapter _parasut;
    private readonly IOutboxWriter _outbox;

    public CreateInvoiceForOrder(IInvoiceRepository repo, IParasutAdapter parasut, IOutboxWriter outbox)
    {
        _repo = repo;
        _parasut = parasut;
        _outbox = outbox;
    }

    public async Task HandleAsync(OrderCreatedIntegrationEvent ev, CancellationToken ct)
    {
        // Idempotency: aynı order için birden çok kez çağrı gelirse yeniden fatura kesmeyelim
        if (await _repo.ExistsByOrderIdAsync(ev.OrderId, ct))
        {}
            return;

        var result = await _parasut.CreateInvoiceAsync(ev.OrderId, ct);
        var invoice = Invoice.Create(ev.OrderId, result.InvoiceNo, result.PdfUrl, "parasut");
        await _repo.AddAsync(invoice, ct);

        var invoiced = new OrderInvoicedIntegrationEvent(
            ev.OrderId, "parasut", result.InvoiceNo, result.PdfUrl, DateTime.UtcNow);
        var payload = JsonSerializer.Serialize(invoiced);

        await _outbox.WriteAsync(typeof(OrderInvoicedIntegrationEvent).FullName!, payload, invoiced.OccurredAtUtc, ct);
    }
}
