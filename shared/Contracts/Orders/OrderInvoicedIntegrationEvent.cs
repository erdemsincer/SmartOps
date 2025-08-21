namespace SmartOps.Contracts.Orders;

public sealed record OrderInvoicedIntegrationEvent(
    Guid OrderId,
    string Provider,
    string InvoiceNo,
    string PdfUrl,
    DateTime OccurredAtUtc
);
