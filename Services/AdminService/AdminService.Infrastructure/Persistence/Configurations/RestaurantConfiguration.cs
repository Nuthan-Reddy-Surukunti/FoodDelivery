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

        builder.OwnsOne(r => r.Address, address =>
        {
            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("Street");

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("City");

            address.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("State");

            address.Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("ZipCode");

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Country");
        });

        builder.OwnsOne(r => r.ContactInfo, contact =>
        {
            contact.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ContactEmail");

            contact.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("ContactPhone");
        });

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

        builder.Ignore(r => r.DomainEvents);
    }
}
