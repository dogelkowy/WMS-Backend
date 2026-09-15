using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Data;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<CWarehouse> Warehouses => Set<CWarehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Ean)
                .IsRequired()
                .HasMaxLength(13);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        // Warehouse
        modelBuilder.Entity<CWarehouse>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(w => w.Description)
                .HasMaxLength(500);

            entity.HasIndex(w => w.Code)
                .IsUnique();
        });
 
        // Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(l => new { l.WarehouseId, l.Code })
                .IsUnique();

            entity.HasOne(l => l.Warehouse)
                .WithMany(w => w.Locations)
                .HasForeignKey(l => l.WarehouseId);
        });
    }
}