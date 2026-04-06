namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a restaurant registration is rejected by an admin
/// </summary>
public sealed class RestaurantRejectedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid RestaurantId { get; }
    public string RestaurantName { get; }
    public string RejectionReason { get; }

    public RestaurantRejectedEvent(Guid restaurantId, string restaurantName, string rejectionReason)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        RestaurantId = restaurantId;
        RestaurantName = restaurantName;
        RejectionReason = rejectionReason ?? throw new ArgumentNullException(nameof(rejectionReason));
    }
}
