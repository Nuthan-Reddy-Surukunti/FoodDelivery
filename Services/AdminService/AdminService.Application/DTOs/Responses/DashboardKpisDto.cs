namespace AdminService.Application.DTOs.Responses;

public class DashboardKpisDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string TotalRevenueCurrency { get; set; } = "INR";
    public int ActivePartners { get; set; }
    public int PendingApprovals { get; set; }
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public string RevenueTodayCurrency { get; set; } = "INR";
    /// <summary>Orders placed yesterday — used to compute daily trend %.</summary>
    public int OrdersYesterday { get; set; }
    /// <summary>Revenue collected yesterday — used to compute revenue trend %.</summary>
    public decimal RevenueYesterday { get; set; }
    /// <summary>Total registered user count.</summary>
    public int TotalUsers { get; set; }
    /// <summary>Per-day order counts for the last 7 days, keyed "yyyy-MM-dd".</summary>
    public Dictionary<string, int> DailyOrderCounts { get; set; } = new();
}