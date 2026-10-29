using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.ListingId).IsRequired();
            entity.Property(o => o.DealerId).IsRequired();
            entity.Property(o => o.FarmerId).IsRequired();
            entity.Property(o => o.Quantity).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(o => o.Price).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(o => o.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(o => o.Status).IsRequired();
            entity.Property(o => o.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(i => i.InvoiceDate).IsRequired();
            entity.Property(i => i.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");

            entity.HasOne(i => i.Order)
                .WithOne(o => o.Invoice)
                .HasForeignKey<Invoice>(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ReceiptNumber).IsRequired().HasMaxLength(50);
            entity.Property(r => r.ReceiptDate).IsRequired();
            entity.Property(r => r.DocumentUrl).HasMaxLength(500);

            entity.HasOne(r => r.Order)
                .WithOne(o => o.Receipt)
                .HasForeignKey<Receipt>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.DealerId).IsRequired();
            entity.Property(r => r.FarmerId).IsRequired();
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(1000);
            entity.Property(r => r.CreatedAt).IsRequired();

            entity.HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
