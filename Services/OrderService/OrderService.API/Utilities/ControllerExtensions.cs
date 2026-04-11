using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Order;
using OrderService.Domain.Enums;

namespace OrderService.API.Utilities;

public static class ControllerExtensions
{
    private static readonly IReadOnlySet<OrderStatus> PartnerAllowedStatusTargets = new HashSet<OrderStatus>
    {
        OrderStatus.RestaurantAccepted,
        OrderStatus.Preparing,
        OrderStatus.ReadyForPickup,
        OrderStatus.RestaurantRejected
    };

    private static readonly IReadOnlySet<OrderStatus> DeliveryAllowedStatusTargets = new HashSet<OrderStatus>
    {
        OrderStatus.PickedUp,
        OrderStatus.OutForDelivery,
        OrderStatus.Delivered
    };

    public static Guid GetCurrentUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    public static string GetCurrentUserRole(this ControllerBase controller)
    {
        return controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
    }

    public static bool IsCurrentUser(this ControllerBase controller, Guid expectedUserId)
    {
        var currentUserId = controller.GetCurrentUserId();
        return currentUserId != Guid.Empty && currentUserId == expectedUserId;
    }

    public static bool CanAccessOrder(this ControllerBase controller, OrderDetailDto order)
    {
        var role = controller.GetCurrentUserRole();
        var currentUserId = controller.GetCurrentUserId();

        return role switch
        {
            "Admin" => true,
            "Customer" => currentUserId != Guid.Empty && order.UserId == currentUserId,
            "DeliveryAgent" => currentUserId != Guid.Empty && order.DeliveryAssignment?.AgentAuthUserId == currentUserId.ToString(),
            "RestaurantPartner" => controller.CanRestaurantPartnerAccessOrder(order),
            _ => false
        };
    }

    public static bool CanRestaurantPartnerAccessOrder(this ControllerBase controller, OrderDetailDto order)
    {
        var restaurantClaim = controller.User.FindFirst("restaurantId")?.Value ?? controller.User.FindFirst("RestaurantId")?.Value;
        if (!Guid.TryParse(restaurantClaim, out var restaurantId))
        {
            return false;
        }

        return order.RestaurantId == restaurantId;
    }

    public static bool CanRoleTransition(this ControllerBase controller, string role, OrderStatus targetStatus)
    {
        return role switch
        {
            "Admin" => true,
            "RestaurantPartner" => PartnerAllowedStatusTargets.Contains(targetStatus),
            "DeliveryAgent" => DeliveryAllowedStatusTargets.Contains(targetStatus),
            _ => false
        };
    }
}