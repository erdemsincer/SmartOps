using Microsoft.EntityFrameworkCore;
using InvoiceService.Domain.Entities;

namespace InvoiceService.Infrastructure.Persistence;

public sealed class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options) { }
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Invoice>(e =>
        {
            e.ToTable("invoices");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId).IsUnique();
            e.Property(x => x.Provider).HasMaxLength(32);
            e.Property(x => x.InvoiceNo).HasMaxLength(64);
            e.Property(x => x.PdfUrl).HasMaxLength(1024);
        });

        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(256).IsRequired();
            e.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        });
    }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }
}
