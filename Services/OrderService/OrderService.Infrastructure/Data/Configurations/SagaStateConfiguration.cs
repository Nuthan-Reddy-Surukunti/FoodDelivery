using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Application.Saga;

namespace OrderService.Infrastructure.Data.Configurations;

public class SagaStateConfiguration : IEntityTypeConfiguration<OrderFulfillmentSagaState>
{
    public void Configure(EntityTypeBuilder<OrderFulfillmentSagaState> builder)
    {
        builder.ToTable("OrderFulfillmentSagaStates");

        builder.HasKey(s => s.CorrelationId);

        builder.Property(s => s.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.PaymentMethod)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(s => s.FailureReason)
            .HasMaxLength(500);

        builder.Property(s => s.TotalAmount)
            .HasColumnType("decimal(18,2)");
    }
}
