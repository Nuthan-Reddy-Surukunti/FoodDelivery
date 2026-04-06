namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the approval status of a restaurant
/// </summary>
public enum RestaurantStatus
{
    /// <summary>
    /// Restaurant registration is pending admin approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Restaurant has been approved and is active
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Restaurant registration was rejected
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Restaurant has been suspended by admin
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Restaurant is temporarily inactive
    /// </summary>
    Inactive = 5
}
