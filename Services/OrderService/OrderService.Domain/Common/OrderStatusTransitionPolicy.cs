namespace OrderService.Domain.Common;

using OrderService.Domain.Enums;

public static class OrderStatusTransitionPolicy
{
    private static readonly IReadOnlyDictionary<OrderStatus, IReadOnlySet<OrderStatus>> AllowedTransitions =
        new Dictionary<OrderStatus, IReadOnlySet<OrderStatus>>
        {
            [OrderStatus.DraftCart] = new HashSet<OrderStatus>
            {
                OrderStatus.CheckoutStarted,
                OrderStatus.CancelRequestedByCustomer
            },
            [OrderStatus.CheckoutStarted] = new HashSet<OrderStatus>
            {
                OrderStatus.PaymentPending,
                OrderStatus.CancelRequestedByCustomer
            },
            [OrderStatus.PaymentPending] = new HashSet<OrderStatus>
            {
                OrderStatus.Paid,
                OrderStatus.PaymentFailed,
                OrderStatus.CancelRequestedByCustomer
            },
            [OrderStatus.Paid] = new HashSet<OrderStatus>
            {
                OrderStatus.RestaurantAccepted,
                OrderStatus.RestaurantRejected,
                OrderStatus.RefundInitiated,
                OrderStatus.CancelRequestedByCustomer
            },
            [OrderStatus.RestaurantAccepted] = new HashSet<OrderStatus>
            {
                OrderStatus.Preparing,
                OrderStatus.CancelRequestedByCustomer,
                OrderStatus.RefundInitiated
            },
            [OrderStatus.Preparing] = new HashSet<OrderStatus>
            {
                OrderStatus.ReadyForPickup
            },
            [OrderStatus.ReadyForPickup] = new HashSet<OrderStatus>
            {
                OrderStatus.PickedUp
            },
            [OrderStatus.PickedUp] = new HashSet<OrderStatus>
            {
                OrderStatus.OutForDelivery
            },
            [OrderStatus.OutForDelivery] = new HashSet<OrderStatus>
            {
                OrderStatus.Delivered
            },
            [OrderStatus.Delivered] = new HashSet<OrderStatus>
            {
                OrderStatus.RefundInitiated
            },
            [OrderStatus.PaymentFailed] = new HashSet<OrderStatus>(),
            [OrderStatus.CancelRequestedByCustomer] = new HashSet<OrderStatus>
            {
                OrderStatus.RefundInitiated,
                OrderStatus.Refunded
            },
            [OrderStatus.RestaurantRejected] = new HashSet<OrderStatus>
            {
                OrderStatus.RefundInitiated,
                OrderStatus.Refunded
            },
            [OrderStatus.RefundInitiated] = new HashSet<OrderStatus>
            {
                OrderStatus.Refunded
            },
            [OrderStatus.Refunded] = new HashSet<OrderStatus>()
        };

    private static readonly ISet<OrderStatus> CustomerCancellableStatuses =
        new HashSet<OrderStatus>
        {
            OrderStatus.DraftCart,
            OrderStatus.CheckoutStarted,
            OrderStatus.PaymentPending,
            OrderStatus.Paid,
            OrderStatus.RestaurantAccepted
        };

    private static readonly ISet<OrderStatus> RefundEligibleStatuses =
        new HashSet<OrderStatus>
        {
            OrderStatus.Paid,
            OrderStatus.RestaurantAccepted,
            OrderStatus.Preparing,
            OrderStatus.ReadyForPickup,
            OrderStatus.PickedUp,
            OrderStatus.OutForDelivery,
            OrderStatus.Delivered,
            OrderStatus.RefundInitiated,
            OrderStatus.Refunded
        };

    private static readonly ISet<OrderStatus> TerminalStatuses =
        new HashSet<OrderStatus>
        {
            OrderStatus.Delivered,
            OrderStatus.PaymentFailed,
            OrderStatus.RestaurantRejected,
            OrderStatus.Refunded
        };

    public static bool CanTransition(OrderStatus currentStatus, OrderStatus targetStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var nextStatuses) &&
               nextStatuses.Contains(targetStatus);
    }

    public static IReadOnlySet<OrderStatus> GetNextStatuses(OrderStatus currentStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var nextStatuses)
            ? nextStatuses
            : new HashSet<OrderStatus>();
    }

    public static bool CanCustomerCancel(OrderStatus status)
    {
        return CustomerCancellableStatuses.Contains(status);
    }

    public static bool IsRefundEligible(OrderStatus status)
    {
        return RefundEligibleStatuses.Contains(status);
    }

    public static bool IsTerminal(OrderStatus status)
    {
        return TerminalStatuses.Contains(status);
    }

    public static bool IsEligibleForDeliveryAssignment(OrderStatus status)
    {
        return status == OrderStatus.ReadyForPickup;
    }
}