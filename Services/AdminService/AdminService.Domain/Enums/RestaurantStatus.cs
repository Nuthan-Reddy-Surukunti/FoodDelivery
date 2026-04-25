namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the approval status of a restaurant.
/// Values must match QuickBite.Shared.Enums.RestaurantStatus for consistency
/// in event-driven architecture across all services.
/// </summary>
public enum RestaurantStatus
{
    /// <summary>
    /// Restaurant registration is pending admin approval
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Restaurant has been approved by admin and is active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Restaurant has been suspended by admin
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Restaurant registration was rejected or permanently inactive
    /// </summary>
    Inactive = 3
}
