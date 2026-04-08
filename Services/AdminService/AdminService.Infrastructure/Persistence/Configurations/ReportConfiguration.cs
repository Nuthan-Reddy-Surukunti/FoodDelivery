using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.TotalOrders)
            .HasColumnName("TotalOrders");

        builder.Property(r => r.TotalRevenue)
            .HasPrecision(18, 2)
            .HasColumnName("TotalRevenue");

        builder.Property(r => r.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasColumnName("Currency");

        builder.Property(r => r.TotalCustomers)
            .HasColumnName("TotalCustomers");

        builder.Property(r => r.TotalRestaurants)
            .HasColumnName("TotalRestaurants");

        builder.Property(r => r.AverageOrderValue)
            .HasColumnName("AverageOrderValue");

        builder.Property(r => r.MetricsStartDate)
            .HasColumnName("MetricsStartDate");

        builder.Property(r => r.MetricsEndDate)
            .HasColumnName("MetricsEndDate");

        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.EndDate)
            .IsRequired();

        builder.Property(r => r.GeneratedAt)
            .IsRequired();

        builder.Property(r => r.FilterCriteria)
            .HasMaxLength(1000);

        builder.HasIndex(r => r.Type);
        builder.HasIndex(r => r.GeneratedAt);
    }
}
