namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a menu item is created
/// </summary>
public sealed class MenuItemCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid MenuItemId { get; }
    public Guid RestaurantId { get; }
    public string MenuItemName { get; }

    public MenuItemCreatedEvent(Guid menuItemId, Guid restaurantId, string menuItemName)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        MenuItemId = menuItemId;
        RestaurantId = restaurantId;
        MenuItemName = menuItemName;
    }
}