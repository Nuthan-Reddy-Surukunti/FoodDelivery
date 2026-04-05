namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the status of an order dispute
/// </summary>
public enum DisputeStatus
{
    /// <summary>
    /// Dispute has been raised and is open
    /// </summary>
    Open = 1,

    /// <summary>
    /// Dispute is under investigation
    /// </summary>
    UnderReview = 2,

    /// <summary>
    /// Dispute has been resolved in favor of customer
    /// </summary>
    ResolvedCustomerFavor = 3,

    /// <summary>
    /// Dispute has been resolved in favor of restaurant
    /// </summary>
    ResolvedRestaurantFavor = 4,

    /// <summary>
    /// Dispute was closed without resolution
    /// </summary>
    Closed = 5
}
