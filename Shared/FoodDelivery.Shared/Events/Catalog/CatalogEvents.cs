namespace FoodDelivery.Shared.Events.Catalog;

/// <summary>
/// Published when a new restaurant is created
/// </summary>
public class RestaurantCreatedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CuisineType { get; set; } = string.Empty;
}

/// <summary>
/// Published when a restaurant is approved
/// </summary>
public class RestaurantApprovedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

/// <summary>
/// Published when a restaurant is rejected
/// </summary>
public class RestaurantRejectedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// Published when a restaurant is deleted by admin
/// </summary>
public class RestaurantDeletedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeletedBy { get; set; } = string.Empty;
}

/// <summary>
/// Published when a menu item is created
/// </summary>
public class MenuItemCreatedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid MenuItemId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Published when a menu item availability changes
/// </summary>
public class MenuItemAvailabilityChangedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid MenuItemId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Published when a menu item is updated
/// </summary>
public class MenuItemUpdatedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid MenuItemId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Published when a menu item is deleted
/// </summary>
public class MenuItemDeletedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid MenuItemId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
}
