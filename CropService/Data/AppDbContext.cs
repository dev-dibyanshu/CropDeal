using Microsoft.EntityFrameworkCore;
using CropService.Models;

namespace CropService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Crop> Crops { get; set; }
    public DbSet<CropListing> CropListings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Crop>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CropName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Category).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Unit).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<CropListing>(entity =>
        {
            entity.HasKey(cl => cl.Id);
            entity.Property(cl => cl.FarmerId).IsRequired();
            entity.Property(cl => cl.Quantity).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(cl => cl.AvailableQuantity).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(cl => cl.ExpectedPrice).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(cl => cl.Location).IsRequired().HasMaxLength(200);
            entity.Property(cl => cl.Status).IsRequired();
            entity.Property(cl => cl.CreatedAt).IsRequired();

            entity.HasOne(cl => cl.Crop)
                .WithMany(c => c.CropListings)
                .HasForeignKey(cl => cl.CropId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
