using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.OwnerId).IsRequired();
        builder.Property(r => r.Street).IsRequired().HasMaxLength(255);
        builder.Property(r => r.City).IsRequired().HasMaxLength(100);
        builder.Property(r => r.State).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ZipCode).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Country).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ContactEmail).IsRequired().HasMaxLength(255);
        builder.Property(r => r.ContactPhone).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>();
        builder.Property(r => r.RejectionReason).HasMaxLength(500);
        builder.HasIndex(r => r.Status);
    }
}
