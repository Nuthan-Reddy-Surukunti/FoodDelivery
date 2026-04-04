using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<OperatingHours> OperatingHours { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Restaurant entity
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(r => r.Description)
                .HasMaxLength(1000);

            entity.Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(r => r.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(r => r.Latitude)
                .IsRequired();

            entity.Property(r => r.Longitude)
                .IsRequired();

            entity.Property(r => r.CuisineType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(r => r.Rating)
                .HasPrecision(3, 2);

            entity.Property(r => r.MinOrderValue)
                .HasPrecision(10, 2);

            entity.Property(r => r.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(RestaurantStatus.Active);

            entity.Property(r => r.ContactPhone)
                .HasMaxLength(20);

            entity.Property(r => r.ContactEmail)
                .HasMaxLength(255);

            entity.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(r => r.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(r => r.Name);
            entity.HasIndex(r => r.CuisineType);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => new { r.Latitude, r.Longitude });
            entity.HasIndex(r => r.CreatedAt);

            // Relationships
            entity.HasMany(r => r.MenuItems)
                .WithOne(m => m.Restaurant)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Categories)
                .WithOne(c => c.Restaurant)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.OperatingHours)
                .WithOne(oh => oh.Restaurant)
                .HasForeignKey(oh => oh.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MenuItem entity
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.RestaurantId)
                .IsRequired();

            entity.Property(m => m.CategoryId)
                .IsRequired();

            entity.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(m => m.Description)
                .HasMaxLength(1000);

            entity.Property(m => m.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(m => m.IsVeg)
                .IsRequired();

            entity.Property(m => m.ImageUrl)
                .HasMaxLength(500);

            entity.Property(m => m.AvailabilityStatus)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(ItemAvailabilityStatus.Available);

            entity.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(m => m.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(m => m.RestaurantId);
            entity.HasIndex(m => m.CategoryId);
            entity.HasIndex(m => m.IsVeg);
            entity.HasIndex(m => m.AvailabilityStatus);

            // Relationships
            entity.HasOne(m => m.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.RestaurantId)
                .IsRequired();

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(c => c.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: Category name unique per restaurant
            entity.HasIndex(c => new { c.RestaurantId, c.Name })
                .IsUnique();

            // Relationships
            entity.HasOne(c => c.Restaurant)
                .WithMany(r => r.Categories)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.MenuItems)
                .WithOne(m => m.Category)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OperatingHours entity
        modelBuilder.Entity<OperatingHours>(entity =>
        {
            entity.HasKey(oh => oh.Id);

            entity.Property(oh => oh.RestaurantId)
                .IsRequired();

            entity.Property(oh => oh.DayOfWeek)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(oh => oh.OpenTime)
                .IsRequired()
                .HasColumnType("time");

            entity.Property(oh => oh.CloseTime)
                .IsRequired()
                .HasColumnType("time");

            entity.Property(oh => oh.IsClosed)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(oh => oh.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(oh => oh.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: One record per restaurant per day
            entity.HasIndex(oh => new { oh.RestaurantId, oh.DayOfWeek })
                .IsUnique();

            // Relationships
            entity.HasOne(oh => oh.Restaurant)
                .WithMany(r => r.OperatingHours)
                .HasForeignKey(oh => oh.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
