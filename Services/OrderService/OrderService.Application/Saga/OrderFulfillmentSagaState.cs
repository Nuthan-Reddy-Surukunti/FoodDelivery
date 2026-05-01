namespace OrderService.Application.Saga;

using MassTransit;
using System;

/// <summary>
/// Persisted state for the OrderFulfillmentSaga.
/// Each row tracks one order's journey through the saga.
/// </summary>
public class OrderFulfillmentSagaState : SagaStateMachineInstance
{
    /// <summary>MassTransit correlation ID — equals the OrderId for easy lookup.</summary>
    public Guid CorrelationId { get; set; }

    /// <summary>Current state name (e.g. "ValidatingOrder", "Confirmed", "Compensating").</summary>
    public string CurrentState { get; set; } = null!;

    // ── Order Context ────────────────────────────────────────────────────────
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public decimal TotalAmount { get; set; }

    /// <summary>"CashOnDelivery" or "Online" — determines if a refund is needed on failure.</summary>
    public string PaymentMethod { get; set; } = "CashOnDelivery";

    /// <summary>Internal Payment record ID — used to retrieve RazorpayPaymentId for refunds.</summary>
    public Guid? PaymentId { get; set; }

    /// <summary>Why the order failed validation (if it did).</summary>
    public string? FailureReason { get; set; }

    // ── Timestamps ───────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
