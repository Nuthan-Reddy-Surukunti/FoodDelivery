using OrderService.Application.DTOs.Profile;
using OrderService.Application.Interfaces;

namespace OrderService.Application.Services;

public class ProfileStatsService : IProfileStatsService
{
    private readonly IOrderPlacementService _orderPlacementService;
    private readonly IUserAddressService _userAddressService;
    private readonly IDeliveryService _deliveryService;

    public ProfileStatsService(
        IOrderPlacementService orderPlacementService,
        IUserAddressService userAddressService,
        IDeliveryService deliveryService)
    {
        _orderPlacementService = orderPlacementService;
        _userAddressService = userAddressService;
        _deliveryService = deliveryService;
    }

    public async Task<ProfileStatsDto> GetProfileStatsAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var stats = new ProfileStatsDto
        {
            Role = role,
            JoinedAt = DateTime.UtcNow // Fallback, usually fetched from AuthService but we'll use UtcNow or fetch if available
        };

        if (role == "Customer")
        {
            var orders = await _orderPlacementService.GetOrdersByUserAsync(userId, false, cancellationToken);
            var activeOrders = orders.Count(o => !new[] { "Delivered", "Cancelled", "RestaurantRejected" }.Contains(o.OrderStatus.ToString()));
            var addresses = await _userAddressService.GetUserAddressesAsync(userId, cancellationToken);

            stats.TotalOrders = orders.Count;
            stats.ActiveOrders = activeOrders;
            stats.SavedAddressesCount = addresses.Count;
        }
        else if (role == "RestaurantPartner")
        {
            // Partner stats depend on restaurantId, which we'll handle in the frontend for now
            // by calling CatalogService first, or we could implement a lookup here if we had the mapping.
        }
        else if (role == "DeliveryAgent")
        {
            var earnings = await _deliveryService.GetEarningsSummaryAsync(userId.ToString(), cancellationToken);
            stats.TotalEarnings = earnings.TotalEarnings;
            stats.DeliveriesCompleted = earnings.TotalDeliveries;
            stats.CurrentStatus = "Active"; // In a real app, this would come from a Presence service
        }
        else if (role == "Admin")
        {
            // For Admin, we can return some high-level system stats if we want, 
            // but the AdminService.Dashboard is better suited for this.
            // We'll leave this for the frontend to call the specialized Admin dashboard API.
        }

        return stats;
    }
}
