using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Street)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("Street");

        builder.Property(r => r.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("City");

        builder.Property(r => r.State)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("State");

        builder.Property(r => r.ZipCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("ZipCode");

        builder.Property(r => r.Country)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Country");

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("ContactEmail");

        builder.Property(r => r.Phone)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("ContactPhone");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.ApprovedAt);

        builder.Property(r => r.RejectedAt);

        builder.Property(r => r.UpdatedAt);

        builder.HasIndex(r => r.Status);
    }
}
