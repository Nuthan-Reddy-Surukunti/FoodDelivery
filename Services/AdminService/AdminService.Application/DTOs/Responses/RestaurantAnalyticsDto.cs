namespace AdminService.Application.DTOs.Responses;

public class RestaurantAnalyticsDto
{
    public int TotalRestaurants { get; set; }
    public int PendingApprovals { get; set; }
    public int ApprovedCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = "INR";
    public Dictionary<Guid, decimal> RevenueByRestaurant { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}
