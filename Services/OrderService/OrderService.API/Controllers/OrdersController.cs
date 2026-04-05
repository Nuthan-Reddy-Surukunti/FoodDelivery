using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderWorkflowService _orderWorkflowService;

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

    public OrdersController(IOrderWorkflowService orderWorkflowService)
    {
        _orderWorkflowService = orderWorkflowService;
    }

    [HttpGet("cart")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(userId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.GetOrCreateCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("cart/items")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.AddCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpPut("cart/items/{cartItemId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid cartItemId,
        [FromBody] UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        request.CartItemId = cartItemId;
        var cart = await _orderWorkflowService.UpdateCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete("cart/items/{cartItemId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RemoveCartItem(
        [FromRoute] Guid cartItemId,
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(userId))
        {
            return Forbid();
        }

        var request = new RemoveCartItemRequestDto
        {
            CartItemId = cartItemId,
            UserId = userId,
            RestaurantId = restaurantId
        };

        var cart = await _orderWorkflowService.RemoveCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete("cart")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ClearCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(userId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.ClearCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("cart/apply-coupon")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ApplyCoupon(
        [FromBody] ApplyCouponRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.ApplyCouponAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpGet("checkout/context")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCheckoutContext(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(userId))
        {
            return Forbid();
        }

        var context = await _orderWorkflowService.GetCheckoutContextAsync(userId, cancellationToken);
        return Ok(context);
    }

    [HttpPost("checkout/validate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ValidateCheckout(
        [FromBody] CheckoutValidationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var isValid = await _orderWorkflowService.ValidateCheckoutAsync(request, cancellationToken);
        return Ok(new { isValid });
    }

    [HttpPost("payments/simulate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SimulatePayment(
        [FromBody] SimulatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var orderToPay = await _orderWorkflowService.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (orderToPay.UserId != currentUserId)
        {
            return Forbid();
        }

        var order = await _orderWorkflowService.SimulatePaymentAsync(request, cancellationToken);
        return Ok(order);
    }

    [HttpPost("orders")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var order = await _orderWorkflowService.PlaceOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, order);
    }

    [HttpGet("orders/{orderId:guid}")]
    [Authorize(Roles = "Customer,RestaurantPartner,DeliveryAgent,Admin")]
    public async Task<IActionResult> GetOrderById(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!CanAccessOrder(order))
        {
            return Forbid();
        }

        var timeline = await _orderWorkflowService.GetOrderTimelineAsync(orderId, cancellationToken);

        return Ok(new
        {
            order,
            timeline
        });
    }

    [HttpGet("orders")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetOrdersByUser(
        [FromQuery] Guid userId,
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        var currentUserRole = this.GetCurrentUserRole();
        if (currentUserRole == "Customer" && !IsCurrentUser(userId))
        {
            return Forbid();
        }

        var orders = await _orderWorkflowService.GetOrdersByUserAsync(userId, activeOnly, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("orders/{orderId:guid}/timeline")]
    [Authorize(Roles = "Customer,RestaurantPartner,DeliveryAgent,Admin")]
    public async Task<IActionResult> GetOrderTimeline(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!CanAccessOrder(order))
        {
            return Forbid();
        }

        var timeline = await _orderWorkflowService.GetOrderTimelineAsync(orderId, cancellationToken);
        return Ok(timeline);
    }

    [HttpPut("orders/{orderId:guid}/status")]
    [Authorize(Roles = "RestaurantPartner,DeliveryAgent,Admin")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        request.OrderId = orderId;

        var role = this.GetCurrentUserRole();
        if (!CanRoleTransition(role, request.TargetStatus))
        {
            return Forbid();
        }

        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!CanAccessOrder(order))
        {
            return Forbid();
        }

        var updated = await _orderWorkflowService.UpdateOrderStatusAsync(request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("orders/{orderId:guid}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var role = this.GetCurrentUserRole();
        if (role == "Customer")
        {
            var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
            if (!CanAccessOrder(order))
            {
                return Forbid();
            }
        }

        var canceled = await _orderWorkflowService.CancelOrderAsync(
            orderId,
            forceByAdmin: role == "Admin",
            cancellationToken);

        return Ok(canceled);
    }

    [HttpPut("orders/{orderId:guid}/assign-delivery")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignDelivery(
        [FromRoute] Guid orderId,
        [FromBody] AssignDeliveryRequestDto request,
        CancellationToken cancellationToken)
    {
        request.OrderId = orderId;
        var updated = await _orderWorkflowService.AssignDeliveryAsync(request, cancellationToken);
        return Ok(updated);
    }

    private bool IsCurrentUser(Guid expectedUserId)
    {
        var currentUserId = this.GetCurrentUserId();
        return currentUserId != Guid.Empty && currentUserId == expectedUserId;
    }

    private bool CanAccessOrder(OrderDetailDto order)
    {
        var role = this.GetCurrentUserRole();
        var currentUserId = this.GetCurrentUserId();

        return role switch
        {
            "Admin" => true,
            "Customer" => currentUserId != Guid.Empty && order.UserId == currentUserId,
            "DeliveryAgent" => currentUserId != Guid.Empty && order.DeliveryAssignment?.DeliveryAgentId == currentUserId,
            "RestaurantPartner" => CanRestaurantPartnerAccessOrder(order),
            _ => false
        };
    }

    private bool CanRestaurantPartnerAccessOrder(OrderDetailDto order)
    {
        var restaurantClaim = User.FindFirst("restaurantId")?.Value ?? User.FindFirst("RestaurantId")?.Value;
        if (!Guid.TryParse(restaurantClaim, out var restaurantId))
        {
            return false;
        }

        return order.RestaurantId == restaurantId;
    }

    private static bool CanRoleTransition(string role, OrderStatus targetStatus)
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
