using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class DeliveryAssignmentConfiguration : IEntityTypeConfiguration<DeliveryAssignment>
{
    public void Configure(EntityTypeBuilder<DeliveryAssignment> builder)
    {
        builder.ToTable("DeliveryAssignments");

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.OrderId)
            .IsRequired();

        builder.Property(assignment => assignment.DeliveryAgentId)
            .IsRequired();

        builder.Property(assignment => assignment.AssignedAt)
            .IsRequired();

        builder.Property(assignment => assignment.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(assignment => assignment.CreatedAt)
            .IsRequired();

        builder.Property(assignment => assignment.UpdatedAt)
            .IsRequired();

        builder.HasIndex(assignment => assignment.OrderId)
            .IsUnique();

        builder.HasIndex(assignment => assignment.DeliveryAgentId);
        builder.HasIndex(assignment => assignment.CurrentStatus);
    }
}
