namespace OrderService.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    private OrderItem() { }

    public OrderItem(string sku, string name, int quantity, decimal price)
    {
        if (quantity <= 0) { throw new ArgumentOutOfRangeException(nameof(quantity)) ;  }
        if (price < 0) { throw new ArgumentOutOfRangeException(nameof(price)); }

        Sku = sku;
        Name = name;
        Quantity = quantity;
        Price = price;
    }
}
