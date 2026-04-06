namespace OrderService.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.UserId).IsRequired();
        builder.Property(order => order.RestaurantId).IsRequired();
        builder.Property(order => order.OrderStatus).HasConversion<int>().IsRequired();
        builder.Property(order => order.DeliveryStreet).HasMaxLength(250);
        builder.Property(order => order.DeliveryCity).HasMaxLength(120);
        builder.Property(order => order.DeliveryPincode).HasMaxLength(10);
        builder.Property(order => order.DeliveryAddressType).HasConversion<int>();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();
        builder.HasIndex(order => order.UserId);
        builder.HasIndex(order => order.RestaurantId);
        builder.HasIndex(order => order.OrderStatus);
        builder.HasIndex(order => order.CreatedAt);
        builder.HasMany(order => order.OrderItems)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(order => order.Payment)
            .WithOne()
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(order => order.DeliveryAssignment)
            .WithOne()
            .HasForeignKey<DeliveryAssignment>(assignment => assignment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
