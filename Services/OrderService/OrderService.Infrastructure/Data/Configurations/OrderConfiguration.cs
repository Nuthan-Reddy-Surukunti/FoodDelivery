using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.UserId)
            .IsRequired();

        builder.Property(order => order.RestaurantId)
            .IsRequired();

        builder.Property(order => order.OrderStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.AppliedCouponCode)
            .HasMaxLength(100);

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.DeliveryAddressLine1)
            .HasMaxLength(250);

        builder.Property(order => order.DeliveryAddressLine2)
            .HasMaxLength(250);

        builder.Property(order => order.DeliveryCity)
            .HasMaxLength(120);

        builder.Property(order => order.DeliveryPostalCode)
            .HasMaxLength(10);

        builder.Property(order => order.DeliveryLatitude);

        builder.Property(order => order.DeliveryLongitude);

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.Property(order => order.UpdatedAt)
            .IsRequired();

        builder.HasIndex(order => order.UserId);
        builder.HasIndex(order => order.RestaurantId);
        builder.HasIndex(order => order.OrderStatus);
        builder.HasIndex(order => order.CreatedAt);

        builder.HasMany(order => order.OrderItems)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(order => order.Payment)
            .WithOne(payment => payment.Order)
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(order => order.DeliveryAssignment)
            .WithOne(assignment => assignment.Order)
            .HasForeignKey<DeliveryAssignment>(assignment => assignment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
