using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.RestaurantId)
            .IsRequired();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.CategoryId)
            .HasMaxLength(100);

        // Configure Money value object
        builder.OwnsOne(m => m.Price, price =>
        {
            price.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Price");

            price.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.ApprovalNotes)
            .HasMaxLength(500);

        builder.Property(m => m.ApprovedBy)
            .HasMaxLength(255);

        builder.Property(m => m.ApprovedAt);

        builder.Property(m => m.RejectionReason)
            .HasMaxLength(500);

        builder.Property(m => m.RejectedBy)
            .HasMaxLength(255);

        builder.Property(m => m.RejectedAt);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        // Indexes for better query performance
        builder.HasIndex(m => m.RestaurantId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.ApprovalStatus);
        builder.HasIndex(m => new { m.RestaurantId, m.Name })
            .IsUnique();

        // Ignore domain events (they are not persisted)
        builder.Ignore(m => m.DomainEvents);
    }
}