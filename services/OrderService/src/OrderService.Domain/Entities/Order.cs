namespace OrderService.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Channel { get; private set; } = "trendyol";
    public string ChannelOrderId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string AddressJson { get; private set; } = "{}";
    public string Status { get; private set; } = "new"; // new|invoiced|shipped|notified|error
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        string channel,
        string channelOrderId,
        decimal totalAmount,
        string customerName,
        string addressJson)
    {
        if (string.IsNullOrWhiteSpace(channelOrderId)){
            throw new ArgumentException("channelOrderId required");
        }

        var o = new Order
        {
            Channel = channel,
            ChannelOrderId = channelOrderId,
            TotalAmount = totalAmount,
            CustomerName = customerName,
            AddressJson = addressJson
        };
        return o;
    }

    public void AddItem(string sku, string name, int qty, decimal price)
    {
        _items.Add(new OrderItem(sku, name, qty, price));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
