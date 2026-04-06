namespace AdminService.Application.DTOs.Responses;

public class ReportDto
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public int TotalRestaurants { get; set; }
    public double AverageOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? FilterCriteria { get; set; }
}
