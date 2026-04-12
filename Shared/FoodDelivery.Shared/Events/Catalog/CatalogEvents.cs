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


