namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the role of a user in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Customer who orders food
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Restaurant owner or manager
    /// </summary>
    Restaurant = 2,

    /// <summary>
    /// Delivery personnel
    /// </summary>
    DeliveryAgent = 3,

    /// <summary>
    /// System administrator with full access
    /// </summary>
    Admin = 4
}
