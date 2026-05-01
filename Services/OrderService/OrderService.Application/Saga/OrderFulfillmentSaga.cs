using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using QuickBite.Shared.Events.Order;
using QuickBite.Shared.Events.Saga;

namespace OrderService.Application.Saga;

/// <summary>
/// The Order Fulfillment Saga.
/// Orchestrates: Payment Confirmed → Validate Restaurant & Items → Confirm or Compensate.
///
/// States:
///   Initial       → (OrderPlacedEvent)    → ValidatingOrder
///   ValidatingOrder → (Succeeded)         → OrderConfirmed
///   ValidatingOrder → (Failed)            → Compensating
///   Compensating  → (auto)               → SagaCompleted (final)
///   OrderConfirmed → stays until manually finalized (order lifecycle continues via status updates)
/// </summary>
public class OrderFulfillmentSaga : MassTransitStateMachine<OrderFulfillmentSagaState>
{
    // ── States ───────────────────────────────────────────────────────────────
    public State ValidatingOrder { get; private set; } = null!;
    public State OrderConfirmed { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;

    // ── Events ───────────────────────────────────────────────────────────────
    public Event<OrderPlacedEvent> OrderPlaced { get; private set; } = null!;
    public Event<OrderValidationSucceededEvent> ValidationSucceeded { get; private set; } = null!;
    public Event<OrderValidationFailedEvent> ValidationFailed { get; private set; } = null!;

    public OrderFulfillmentSaga(ILogger<OrderFulfillmentSaga> logger)
    {
        // Bind CurrentState string property to the state machine
        InstanceState(x => x.CurrentState);

        // Correlate all events by OrderId (= CorrelationId)
        Event(() => OrderPlaced, x =>
        {
            x.CorrelateById(m => m.Message.OrderId);
            x.SelectId(m => m.Message.OrderId);
        });

        Event(() => ValidationSucceeded,
            x => x.CorrelateById(m => m.Message.OrderId));

        Event(() => ValidationFailed,
            x => x.CorrelateById(m => m.Message.OrderId));

        // ── Initial → ValidatingOrder ─────────────────────────────────────
        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    // Capture everything the Saga needs to make decisions later
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.RestaurantId = ctx.Message.RestaurantId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                    ctx.Saga.PaymentMethod = ctx.Message.PaymentMethod;
                    ctx.Saga.PaymentId = ctx.Message.PaymentId;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;

                    logger.LogInformation(
                        "[Saga] Order {OrderId} received — PaymentMethod: {PaymentMethod}. Sending to validation.",
                        ctx.Message.OrderId, ctx.Message.PaymentMethod);
                })
                // Dispatch ValidateOrderCommand → picked up by ValidateOrderConsumer
                .Publish(ctx => new ValidateOrderCommand
                {
                    OrderId = ctx.Saga.OrderId,
                    RestaurantId = ctx.Saga.RestaurantId,
                    Items = ctx.Message.Items.Select(i => new ValidateOrderItem
                    {
                        MenuItemId = i.MenuItemId,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .TransitionTo(ValidatingOrder)
        );

        // ── ValidatingOrder → OrderConfirmed (happy path) ─────────────────
        During(ValidatingOrder,
            When(ValidationSucceeded)
                .Then(ctx =>
                {
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                    logger.LogInformation(
                        "[Saga] Order {OrderId} VALIDATED successfully — Order confirmed.",
                        ctx.Saga.OrderId);
                })
                .TransitionTo(OrderConfirmed),

            // ── ValidatingOrder → Compensating (failure path) ─────────────
            When(ValidationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;

                    logger.LogWarning(
                        "[Saga] Order {OrderId} FAILED validation — Reason: {Reason}. Triggering compensation.",
                        ctx.Saga.OrderId, ctx.Message.Reason);
                })
                // Dispatch CompensateOrderCommand → picked up by CompensateOrderConsumer
                .Publish(ctx => new CompensateOrderCommand
                {
                    OrderId = ctx.Saga.OrderId,
                    Reason = ctx.Saga.FailureReason ?? "Validation failed",
                    ShouldRefund = ctx.Saga.PaymentMethod != "CashOnDelivery",
                    PaymentId = ctx.Saga.PaymentId,
                    Amount = ctx.Saga.TotalAmount
                })
                .TransitionTo(Compensating)
        );

        // ── Compensating: Saga is done (final state) ──────────────────────
        // The CompensateOrderConsumer handles the actual DB/refund work.
        // We mark the saga as completed so EF removes the row.
        During(Compensating,
            Ignore(OrderPlaced),
            Ignore(ValidationSucceeded),
            Ignore(ValidationFailed)
        );

        // Mark Compensating and OrderConfirmed as final (saga instance can be cleaned up)
        SetCompletedWhenFinalized();
    }
}
