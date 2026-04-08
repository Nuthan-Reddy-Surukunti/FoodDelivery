namespace AdminService.Application.DTOs.Responses;

public class DashboardKpisDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string TotalRevenueCurrency { get; set; } = "USD";
    public int ActivePartners { get; set; }
    public int PendingApprovals { get; set; }
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public string RevenueTodayCurrency { get; set; } = "USD";
}