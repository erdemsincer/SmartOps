namespace InvoiceService.Domain.Entities;

public sealed class Invoice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = "parasut";
    public string InvoiceNo { get; private set; } = string.Empty;
    public string PdfUrl { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Invoice() { }

    public static Invoice Create(Guid orderId, string invoiceNo, string pdfUrl, string provider = "parasut")
    {
        if (orderId == Guid.Empty) throw new ArgumentException(nameof(orderId));
        if (string.IsNullOrWhiteSpace(invoiceNo)) throw new ArgumentException(nameof(invoiceNo));
        return new Invoice
        {
            OrderId = orderId,
            Provider = provider,
            InvoiceNo = invoiceNo,
            PdfUrl = pdfUrl
        };
    }
}
