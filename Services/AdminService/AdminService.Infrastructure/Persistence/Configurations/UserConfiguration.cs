using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.OwnsOne(u => u.ContactInfo, contact =>
        {
            contact.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ContactEmail");

            contact.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("ContactPhone");
        });

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.LastLoginAt);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Ignore(u => u.DomainEvents);
    }
}
