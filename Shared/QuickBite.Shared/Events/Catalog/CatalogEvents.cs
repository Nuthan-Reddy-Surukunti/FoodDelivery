namespace QuickBite.Shared.Events.Catalog;

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
/// Published when a restaurant is updated
/// </summary>
public class RestaurantUpdatedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CuisineType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
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
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string AvailabilityStatus { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
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
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string AvailabilityStatus { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
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
}
