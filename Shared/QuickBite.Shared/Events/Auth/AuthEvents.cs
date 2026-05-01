namespace QuickBite.Shared.Events.Auth;

/// <summary>
/// Published when a new user registers (Customer, RestaurantPartner, DeliveryAgent)
/// </summary>
public class UserRegisteredEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Customer, RestaurantPartner, DeliveryAgent, Admin
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Published when a restaurant is approved by an admin
/// </summary>
public class RestaurantApprovedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public Guid ApprovedByAdminId { get; set; }
    public DateTime ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
}

/// <summary>
/// Published when a restaurant is rejected by an admin
/// </summary>
public class RestaurantRejectedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid RestaurantId { get; set; }
    public Guid RejectedByAdminId { get; set; }
    public DateTime RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Published when a user account is deleted
/// </summary>
public class UserDeletedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Published when a user updates their profile
/// </summary>
public class UserUpdatedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// Published when a user's active status is toggled by an admin
/// </summary>
public class UserStatusChangedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
}
