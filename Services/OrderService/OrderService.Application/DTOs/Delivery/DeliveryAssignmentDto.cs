namespace OrderService.Application.DTOs.Delivery;

using OrderService.Domain.Enums;

public class DeliveryAssignmentDto
{
    public Guid DeliveryAssignmentId { get; set; }

    public Guid DeliveryAgentId { get; set; }

    /// <summary>
    /// The AuthService user ID for the assigned delivery agent (used for JWT token comparison).
    /// </summary>
    public string? AgentAuthUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? PickedUpAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DeliveryStatus CurrentStatus { get; set; }

    public string? AgentName { get; set; }

    public string? AgentEmail { get; set; }

    public string? AgentPhone { get; set; }
}