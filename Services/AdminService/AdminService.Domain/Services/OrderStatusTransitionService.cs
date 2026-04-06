using AdminService.Domain.Enums;

namespace AdminService.Domain.Services;

public class OrderStatusTransitionService
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
        { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
        { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Ready, OrderStatus.Cancelled } },
        { OrderStatus.Ready, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Cancelled } },
        { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
        { OrderStatus.Delivered, new List<OrderStatus> { } }, // Final state, no transitions allowed naturally
        { OrderStatus.Cancelled, new List<OrderStatus> { } }  // Final state, no transitions allowed naturally
    };

    public static bool IsTransitionAllowed(OrderStatus fromStatus, OrderStatus toStatus)
    {
        return AllowedTransitions.ContainsKey(fromStatus) && 
               AllowedTransitions[fromStatus].Contains(toStatus);
    }

    public static bool IsAdminOverrideRequired(OrderStatus fromStatus, OrderStatus toStatus)
    {
        // Admin can override most transitions, but we require reason for override scenarios
        return !IsTransitionAllowed(fromStatus, toStatus);
    }

    public static List<OrderStatus> GetAllowedTransitions(OrderStatus fromStatus)
    {
        return AllowedTransitions.ContainsKey(fromStatus) 
            ? AllowedTransitions[fromStatus] 
            : new List<OrderStatus>();
    }
}