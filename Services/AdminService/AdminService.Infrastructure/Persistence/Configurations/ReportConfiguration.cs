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

        builder.OwnsOne(r => r.Metrics, metrics =>
        {
            metrics.Property(m => m.TotalOrders)
                .HasColumnName("TotalOrders");

            metrics.OwnsOne(m => m.TotalRevenue, revenue =>
            {
                revenue.Property(rv => rv.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName("TotalRevenue");

                revenue.Property(rv => rv.Currency)
                    .HasMaxLength(3)
                    .HasColumnName("Currency");
            });

            metrics.Property(m => m.TotalCustomers)
                .HasColumnName("TotalCustomers");

            metrics.Property(m => m.TotalRestaurants)
                .HasColumnName("TotalRestaurants");

            metrics.Property(m => m.AverageOrderValue)
                .HasColumnName("AverageOrderValue");

            metrics.Property(m => m.StartDate)
                .HasColumnName("MetricsStartDate");

            metrics.Property(m => m.EndDate)
                .HasColumnName("MetricsEndDate");
        });

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

        builder.Ignore(r => r.DomainEvents);
    }
}
