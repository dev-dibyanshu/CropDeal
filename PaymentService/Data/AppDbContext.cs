using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OrderId).IsRequired();
            entity.Property(p => p.DealerId).IsRequired();
            entity.Property(p => p.FarmerId).IsRequired();
            entity.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(p => p.PaymentStatus).IsRequired();
            entity.Property(p => p.TransactionReference).IsRequired().HasMaxLength(100);
            entity.Property(p => p.CreatedAt).IsRequired();
            
            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.DealerId);
            entity.HasIndex(p => p.FarmerId);
            entity.HasIndex(p => p.TransactionReference).IsUnique();
        });
    }
}
