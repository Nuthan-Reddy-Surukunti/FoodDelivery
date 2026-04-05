using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.MenuItemId)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.UnitPriceSnapshot)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.CustomizationNotes)
            .HasMaxLength(500);

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.UpdatedAt)
            .IsRequired();

        builder.Ignore(item => item.Subtotal);

        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.MenuItemId);
    }
}
