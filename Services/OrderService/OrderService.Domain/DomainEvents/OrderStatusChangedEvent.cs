namespace OrderService.Domain.DomainEvents;

using OrderService.Domain.Enums;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    DateTime ChangedAt,
    Guid? ChangedBy);