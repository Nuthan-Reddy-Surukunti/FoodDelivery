namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;

public class DeliveryAssignment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid DeliveryAgentId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DeliveryStatus CurrentStatus { get; set; } = DeliveryStatus.PickupPending;
}