namespace OrderService.Domain.DomainEvents;

using OrderService.Domain.ValueObjects;

public sealed record RefundInitiatedEvent(
    Guid PaymentId,
    Guid OrderId,
    Money RefundAmount,
    string? Reason,
    DateTime OccurredAt);