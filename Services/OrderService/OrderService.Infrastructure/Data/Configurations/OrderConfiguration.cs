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

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.Property(order => order.UpdatedAt)
            .IsRequired();

        builder.HasIndex(order => order.UserId);
        builder.HasIndex(order => order.RestaurantId);
        builder.HasIndex(order => order.OrderStatus);
        builder.HasIndex(order => order.CreatedAt);

        builder.Ignore(order => order.AppliedCoupon);
        builder.Ignore(order => order.OrderItems);

        builder.HasMany<OrderItem>("_orderItems")
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_orderItems").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(order => order.Payment)
            .WithOne()
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(order => order.DeliveryAssignment)
            .WithOne()
            .HasForeignKey<DeliveryAssignment>(assignment => assignment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(order => order.DeliveryAddress, address =>
        {
            address.Property(value => value.Street)
                .HasColumnName("DeliveryStreet")
                .HasMaxLength(250);

            address.Property(value => value.City)
                .HasColumnName("DeliveryCity")
                .HasMaxLength(120);

            address.Property(value => value.Pincode)
                .HasColumnName("DeliveryPincode")
                .HasMaxLength(10);

            address.Property(value => value.AddressType)
                .HasColumnName("DeliveryAddressType")
                .HasConversion<int>();

            address.Property(value => value.Latitude)
                .HasColumnName("DeliveryLatitude");

            address.Property(value => value.Longitude)
                .HasColumnName("DeliveryLongitude");
        });
    }
}
