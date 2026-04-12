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
