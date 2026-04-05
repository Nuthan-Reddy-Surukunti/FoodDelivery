using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.OrderId)
            .IsRequired();

        builder.Property(payment => payment.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(payment => payment.PaymentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(payment => payment.TransactionId)
            .HasMaxLength(150);

        builder.Property(payment => payment.FailureReason)
            .HasMaxLength(500);

        builder.Property(payment => payment.ProcessedAt);

        builder.Property(payment => payment.CreatedAt)
            .IsRequired();

        builder.Property(payment => payment.UpdatedAt)
            .IsRequired();

        builder.HasIndex(payment => payment.OrderId)
            .IsUnique();

        builder.OwnsOne(payment => payment.Amount, amount =>
        {
            amount.Property(money => money.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2);

            amount.Property(money => money.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(payment => payment.RefundedAmount, refunded =>
        {
            refunded.Property(money => money.Amount)
                .HasColumnName("RefundedAmount")
                .HasPrecision(18, 2);

            refunded.Property(money => money.Currency)
                .HasColumnName("RefundedCurrency")
                .HasMaxLength(3);
        });
    }
}
