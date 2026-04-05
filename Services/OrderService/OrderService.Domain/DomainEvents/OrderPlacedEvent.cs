namespace OrderService.Domain.DomainEvents;

using OrderService.Domain.ValueObjects;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    Guid RestaurantId,
    Money OrderTotal,
    DateTime OccurredAt);