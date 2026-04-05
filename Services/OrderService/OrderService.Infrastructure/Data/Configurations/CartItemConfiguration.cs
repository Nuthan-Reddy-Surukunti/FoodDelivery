using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.CartId)
            .IsRequired();

        builder.Property(item => item.MenuItemId)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.PriceSnapshot)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.CustomizationNotes)
            .HasMaxLength(500);

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.Property(item => item.UpdatedAt)
            .IsRequired();

        builder.Ignore(item => item.Subtotal);

        builder.HasIndex(item => item.CartId);
        builder.HasIndex(item => item.MenuItemId);
    }
}
