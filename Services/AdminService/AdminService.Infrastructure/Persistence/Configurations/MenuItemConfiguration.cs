using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.RestaurantId).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(255);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.CategoryId).HasMaxLength(100);
        builder.Property(m => m.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(m => m.Currency).IsRequired().HasMaxLength(3);
        builder.Property(m => m.Status).IsRequired().HasConversion<string>();
        builder.Property(m => m.ApprovalStatus).IsRequired().HasConversion<string>();
        builder.Property(m => m.ApprovalNotes).HasMaxLength(500);
        builder.Property(m => m.ApprovedBy).HasMaxLength(255);
        builder.Property(m => m.RejectionReason).HasMaxLength(500);
        builder.Property(m => m.RejectedBy).HasMaxLength(255);
        builder.HasIndex(m => m.RestaurantId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.ApprovalStatus);
        builder.HasIndex(m => new { m.RestaurantId, m.Name }).IsUnique();
    }
}
