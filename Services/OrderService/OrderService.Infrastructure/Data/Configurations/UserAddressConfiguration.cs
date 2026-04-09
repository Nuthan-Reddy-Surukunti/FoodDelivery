using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(address => address.Id);

        builder.Property(address => address.UserId)
            .IsRequired();

        builder.Property(address => address.AddressLine1)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(address => address.AddressLine2)
            .HasMaxLength(250);

        builder.Property(address => address.City)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(address => address.State)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(address => address.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(address => address.AddressType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(address => address.IsDefault)
            .IsRequired();

        builder.Property(address => address.CreatedAt)
            .IsRequired();

        builder.Property(address => address.UpdatedAt)
            .IsRequired();

        builder.HasIndex(address => address.UserId);
        builder.HasIndex(address => new { address.UserId, address.IsDefault });
    }
}
