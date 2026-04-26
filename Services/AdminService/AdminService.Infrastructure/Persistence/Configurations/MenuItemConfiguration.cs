using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        builder.Property(m => m.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(m => m.AvailabilityStatus)
            .HasMaxLength(50);

        builder.Property(m => m.CategoryName)
            .HasMaxLength(100);

        builder.HasOne(m => m.Restaurant)
            .WithMany()
            .HasForeignKey(m => m.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
