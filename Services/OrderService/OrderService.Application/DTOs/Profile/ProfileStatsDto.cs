namespace OrderService.Application.DTOs.Profile;

public class ProfileStatsDto
{
    // Common
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    // Customer Stats
    public int? TotalOrders { get; set; }
    public int? ActiveOrders { get; set; }
    public int? SavedAddressesCount { get; set; }

    // Restaurant Partner Stats
    public decimal? LifetimeRevenue { get; set; }
    public double? AverageRating { get; set; }
    public int? MenuItemsCount { get; set; }
    public string? RestaurantName { get; set; }

    // Delivery Agent Stats
    public decimal? TotalEarnings { get; set; }
    public int? DeliveriesCompleted { get; set; }
    public string? CurrentStatus { get; set; }
    public string? VehicleDetails { get; set; }

    // Admin Stats
    public int? SystemUsersCount { get; set; }
    public decimal? SystemRevenue { get; set; }
}
