using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminService.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.UserName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityId)
                .IsRequired();

            builder.Property(a => a.OldValues)
                .IsRequired(false);

            builder.Property(a => a.NewValues)
                .IsRequired(false);

            builder.Property(a => a.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.IPAddress)
                .IsRequired(false)
                .HasMaxLength(45); // Supports both IPv4 and IPv6

            builder.Property(a => a.UserAgent)
                .IsRequired(false)
                .HasMaxLength(1000);

            // Indexes for common queries
            builder.HasIndex(a => a.EntityId)
                .HasDatabaseName("IX_AuditLogs_EntityId");

            builder.HasIndex(a => a.EntityType)
                .HasDatabaseName("IX_AuditLogs_EntityType");

            builder.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(a => a.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            builder.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");

            builder.ToTable("AuditLogs");
        }
    }
}