namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a menu item is rejected by an admin
/// </summary>
public sealed class MenuItemRejectedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid MenuItemId { get; }
    public Guid RestaurantId { get; }
    public string MenuItemName { get; }
    public string RejectionReason { get; }
    public string RejectedBy { get; }

    public MenuItemRejectedEvent(Guid menuItemId, Guid restaurantId, string menuItemName, string rejectedBy, string rejectionReason)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        MenuItemId = menuItemId;
        RestaurantId = restaurantId;
        MenuItemName = menuItemName;
        RejectedBy = rejectedBy;
        RejectionReason = rejectionReason ?? throw new ArgumentNullException(nameof(rejectionReason));
    }
}