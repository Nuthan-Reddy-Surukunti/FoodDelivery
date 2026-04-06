namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a menu item is approved by an admin
/// </summary>
public sealed class MenuItemApprovedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid MenuItemId { get; }
    public Guid RestaurantId { get; }
    public string MenuItemName { get; }
    public string? ApprovalNotes { get; }
    public string ApprovedBy { get; }

    public MenuItemApprovedEvent(Guid menuItemId, Guid restaurantId, string menuItemName, string approvedBy, string? approvalNotes = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        MenuItemId = menuItemId;
        RestaurantId = restaurantId;
        MenuItemName = menuItemName;
        ApprovedBy = approvedBy;
        ApprovalNotes = approvalNotes;
    }
}