namespace OrderService.Application.Incoming;

public sealed class TrendyolWebhookDto
{
    public string ChannelOrderId { get; set; } = string.Empty;     // TY sipariş no
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string AddressJson { get; set; } = "{}";
    public List<TrendyolItemDto> Items { get; set; } = new();
}

public sealed class TrendyolItemDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
