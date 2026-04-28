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
}
