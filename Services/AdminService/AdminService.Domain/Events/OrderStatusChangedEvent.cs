using AdminService.Domain.Enums;

namespace AdminService.Domain.Events;

public sealed class OrderStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid OrderId { get; }
    public OrderStatus PreviousStatus { get; }
    public OrderStatus NewStatus { get; }
    public string Reason { get; }
    public string AdminUserId { get; }
    public decimal? RefundAmount { get; }

    public OrderStatusChangedEvent(
        Guid orderId, 
        OrderStatus previousStatus, 
        OrderStatus newStatus, 
        string reason, 
        string adminUserId,
        decimal? refundAmount = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        OrderId = orderId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        AdminUserId = adminUserId;
        RefundAmount = refundAmount;
    }
}