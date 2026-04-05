namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;

public class DeliveryAssignment : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid DeliveryAgentId { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public DateTime? PickedUpAt { get; private set; }

    public DateTime? DeliveredAt { get; private set; }

    public DeliveryStatus CurrentStatus { get; private set; } = DeliveryStatus.PickupPending;

    public DeliveryAssignment(Guid orderId, Guid deliveryAgentId, DateTime assignedAt)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        if (deliveryAgentId == Guid.Empty)
        {
            throw new ArgumentException("Delivery agent ID is required.", nameof(deliveryAgentId));
        }

        OrderId = orderId;
        DeliveryAgentId = deliveryAgentId;
        AssignedAt = assignedAt;
    }

    public void MarkAsPickedUp(DateTime pickedUpAt)
    {
        if (pickedUpAt <= AssignedAt)
        {
            throw new InvalidOperationException("Pickup time must be later than assignment time.");
        }

        PickedUpAt = pickedUpAt;
        CurrentStatus = DeliveryStatus.PickedUp;
        Touch();
    }

    public void MarkAsInTransit(DateTime inTransitAt)
    {
        if (!PickedUpAt.HasValue)
        {
            throw new InvalidOperationException("Order must be picked up before marking in transit.");
        }

        if (inTransitAt < PickedUpAt.Value)
        {
            throw new InvalidOperationException("In-transit time cannot be earlier than pickup time.");
        }

        CurrentStatus = DeliveryStatus.InTransit;
        Touch();
    }

    public void MarkAsDelivered(DateTime deliveredAt)
    {
        if (!PickedUpAt.HasValue)
        {
            throw new InvalidOperationException("Order must be picked up before delivery completion.");
        }

        if (deliveredAt <= PickedUpAt.Value)
        {
            throw new InvalidOperationException("Delivery time must be later than pickup time.");
        }

        DeliveredAt = deliveredAt;
        CurrentStatus = DeliveryStatus.Delivered;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}