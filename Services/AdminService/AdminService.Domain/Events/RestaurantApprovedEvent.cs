namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a restaurant is approved by an admin
/// </summary>
public sealed class RestaurantApprovedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid RestaurantId { get; }
    public string RestaurantName { get; }
    public string? ApprovalNotes { get; }

    public RestaurantApprovedEvent(Guid restaurantId, string restaurantName, string? approvalNotes = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        RestaurantId = restaurantId;
        RestaurantName = restaurantName;
        ApprovalNotes = approvalNotes;
    }
}
