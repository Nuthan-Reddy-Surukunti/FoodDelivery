namespace OrderService.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.OrderId).IsRequired();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
        builder.Property(payment => payment.PaymentMethod).HasConversion<int>().IsRequired();
        builder.Property(payment => payment.PaymentStatus).HasConversion<int>().IsRequired();
        builder.Property(payment => payment.TransactionId).HasMaxLength(150);
        builder.Property(payment => payment.FailureReason).HasMaxLength(500);
        builder.Property(payment => payment.RefundedAmount).HasPrecision(18, 2);
        builder.Property(payment => payment.RefundedCurrency).HasMaxLength(3);
        builder.Property(payment => payment.CreatedAt).IsRequired();
        builder.Property(payment => payment.UpdatedAt).IsRequired();
        builder.HasIndex(payment => payment.OrderId).IsUnique();
    }
}
