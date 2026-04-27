using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
