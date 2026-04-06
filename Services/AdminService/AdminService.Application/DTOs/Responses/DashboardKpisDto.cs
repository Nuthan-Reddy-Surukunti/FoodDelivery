using AdminService.Domain.ValueObjects;

namespace AdminService.Application.DTOs.Responses;

public class DashboardKpisDto
{
    public int TotalOrders { get; set; }
    public Money TotalRevenue { get; set; } = Money.Zero("USD");
    public int ActivePartners { get; set; }
    public int PendingApprovals { get; set; }
    public int OrdersToday { get; set; }
    public Money RevenueToday { get; set; } = Money.Zero("USD");
}