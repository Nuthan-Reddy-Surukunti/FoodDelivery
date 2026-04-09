using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Configurations;

public class DeliveryAgentConfiguration : IEntityTypeConfiguration<DeliveryAgent>
{
    public void Configure(EntityTypeBuilder<DeliveryAgent> builder)
    {
        builder.ToTable("DeliveryAgents");

        builder.HasKey(agent => agent.Id);

        builder.Property(agent => agent.AuthUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(agent => agent.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(agent => agent.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(agent => agent.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(agent => agent.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(agent => agent.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(agent => agent.CreatedAt)
            .IsRequired();

        builder.Property(agent => agent.UpdatedAt)
            .IsRequired();

        // Unique constraint on AuthUserId to ensure each AuthService user is only one agent in OrderService
        builder.HasIndex(agent => agent.AuthUserId)
            .IsUnique();

        // Index for queries by ActiveAndVerified status
        builder.HasIndex(agent => new { agent.IsActive, agent.IsEmailVerified });

        // Relationship: DeliveryAgent has many DeliveryAssignments
        builder.HasMany(agent => agent.DeliveryAssignments)
            .WithOne()
            .HasForeignKey(assignment => assignment.DeliveryAgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
