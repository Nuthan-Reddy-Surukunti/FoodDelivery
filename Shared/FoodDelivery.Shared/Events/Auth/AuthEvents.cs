namespace FoodDelivery.Shared.Events.Auth;

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
}

/// <summary>
/// Published when a RestaurantPartner or Admin account is approved
/// </summary>
public class UserApprovedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}

/// <summary>
/// Published when a RestaurantPartner or Admin account is rejected
/// </summary>
public class UserRejectedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public string RejectedBy { get; set; } = string.Empty;
}

/// <summary>
/// Published when a user account is deleted
/// </summary>
public class UserDeletedEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int EventVersion { get; set; } = 1;

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
