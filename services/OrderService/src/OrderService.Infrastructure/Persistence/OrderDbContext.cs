using System;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Orders
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("orders");
            b.HasKey(x => x.Id);

            b.Property(x => x.Channel).HasMaxLength(32).IsRequired();
            b.Property(x => x.ChannelOrderId).HasMaxLength(64).IsRequired();
            b.HasIndex(x => new { x.Channel, x.ChannelOrderId }).IsUnique();

            b.Property(x => x.CustomerName).HasMaxLength(200);
            b.Property(x => x.AddressJson).HasColumnType("jsonb");
            b.Property(x => x.Status).HasMaxLength(32).HasDefaultValue("new");
            b.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)");
            b.Property(x => x.CreatedAt);
            b.Property(x => x.UpdatedAt);

            // Items navigation -> backing field _items
            b.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey("order_id")
             .OnDelete(DeleteBehavior.Cascade);

            b.Metadata
             .FindNavigation(nameof(Order.Items))!
             .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // OrderItems
        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("order_items");
            b.HasKey(x => x.Id);

            b.Property(x => x.Sku).HasMaxLength(64);
            b.Property(x => x.Name).HasMaxLength(256);
            b.Property(x => x.Quantity);
            b.Property(x => x.Price).HasColumnType("numeric(18,2)");

            // FK sütununu görünür yap
            b.Property<Guid>("order_id");
        });

        // Outbox
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type)
                .HasMaxLength(256)
                .IsRequired();

            // PostgreSQL için JSONB
            b.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            b.Property(x => x.Attempt)
                .HasDefaultValue(0);

            // PostgreSQL UTC now
            b.Property(x => x.OccurredAtUtc)
                .HasDefaultValueSql("timezone('utc', now())");

            b.Property(x => x.ProcessedAtUtc);
            b.Property(x => x.Error);

            // Sorgu/polling indexleri
            b.HasIndex(x => x.ProcessedAtUtc);
            b.HasIndex(x => x.OccurredAtUtc);
            b.HasIndex(x => x.Type);
        });
    }
}
