using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Type).IsRequired().HasConversion<string>();
        builder.Property(r => r.TotalOrders);
        builder.Property(r => r.TotalRevenue).HasPrecision(18, 2);
        builder.Property(r => r.Currency).HasMaxLength(3);
        builder.Property(r => r.TotalCustomers);
        builder.Property(r => r.TotalRestaurants);
        builder.Property(r => r.AverageOrderValue);
        builder.Property(r => r.MetricsStartDate);
        builder.Property(r => r.MetricsEndDate);
        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate).IsRequired();
        builder.Property(r => r.GeneratedAt).IsRequired();
        builder.Property(r => r.FilterCriteria).HasMaxLength(1000);
        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.GeneratedAt);
    }
}
