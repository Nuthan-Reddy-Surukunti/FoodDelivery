using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

/// <summary>
/// Report aggregate root representing generated reports
/// </summary>
public class Report
{
    public Guid Id { get; set; }
    public ReportType Type { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalCustomers { get; set; }
    public int TotalRestaurants { get; set; }
    public double AverageOrderValue { get; set; }
    public DateTime MetricsStartDate { get; set; }
    public DateTime MetricsEndDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? FilterCriteria { get; set; }
}
