namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

/// <summary>
/// Represents a delivery agent in the system.
/// Agents are synced from AuthService via RabbitMQ events when users register as delivery agents.
/// </summary>
public class DeliveryAgent : BaseEntity
{
    /// <summary>
    /// The AuthService user ID (string GUID) for this delivery agent.
    /// </summary>
    public string AuthUserId { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the delivery agent.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the delivery agent.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether the delivery agent is active and available for assignments.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the agent's email has been verified in AuthService.
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// Phone number of the delivery agent.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Navigation property: Delivery assignments for this agent.
    /// </summary>
    public ICollection<DeliveryAssignment> DeliveryAssignments { get; set; } = [];
}
