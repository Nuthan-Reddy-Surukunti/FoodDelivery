namespace OrderService.Application.DTOs.Delivery;

using OrderService.Domain.Enums;

public class DeliveryAssignmentDto
{
    public Guid DeliveryAssignmentId { get; set; }

    public Guid DeliveryAgentId { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? PickedUpAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DeliveryStatus CurrentStatus { get; set; }
}