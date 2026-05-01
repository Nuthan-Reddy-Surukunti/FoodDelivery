namespace OrderService.Application.DTOs.Order;

public class PartnerStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PreparingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public Dictionary<string, decimal> DailyRevenue { get; set; } = new();
    /// <summary>Name of the most-ordered menu item for this restaurant (null if no orders yet).</summary>
    public string? TopSellingItem { get; set; }
    /// <summary>Average time in minutes from order placed to restaurant acceptance (null if not enough data).</summary>
    public double? AvgPrepTimeMinutes { get; set; }
}
