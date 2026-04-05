namespace OrderService.Domain.DomainEvents;

using OrderService.Domain.ValueObjects;

public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    Money Amount,
    DateTime OccurredAt);