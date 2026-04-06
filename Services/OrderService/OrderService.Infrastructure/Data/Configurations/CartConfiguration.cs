using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.UserId)
            .IsRequired();

        builder.Property(cart => cart.RestaurantId)
            .IsRequired();

        builder.Property(cart => cart.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cart => cart.AppliedCouponCode)
            .HasMaxLength(100);

        builder.Property(cart => cart.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cart => cart.CreatedAt)
            .IsRequired();

        builder.Property(cart => cart.UpdatedAt)
            .IsRequired();

        builder.HasIndex(cart => cart.UserId);
        builder.HasIndex(cart => cart.RestaurantId);
        builder.HasIndex(cart => cart.Status);

        builder.HasIndex(cart => new { cart.UserId, cart.RestaurantId })
            .IsUnique()
            .HasFilter($"[{nameof(Cart.Status)}] = {(int)CartStatus.Active}")
            .HasDatabaseName("IX_Carts_UserRestaurant_Active");

        builder.HasMany(cart => cart.Items)
            .WithOne(item => item.Cart)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
