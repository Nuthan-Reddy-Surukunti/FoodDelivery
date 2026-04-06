using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderWorkflowService _orderWorkflowService;

    public OrdersController(IOrderWorkflowService orderWorkflowService)
    {
        _orderWorkflowService = orderWorkflowService;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var order = await _orderWorkflowService.PlaceOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = order.OrderId }, order);
    }

    [HttpGet("{orderId:guid}")]
    [Authorize(Roles = "Customer,RestaurantPartner,DeliveryAgent,Admin")]
    public async Task<IActionResult> GetOrderById(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!this.CanAccessOrder(order))
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

    [HttpGet]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetOrdersByUser(
        [FromQuery] Guid userId,
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        var currentUserRole = this.GetCurrentUserRole();
        if (currentUserRole == "Customer" && !this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var orders = await _orderWorkflowService.GetOrdersByUserAsync(userId, activeOnly, cancellationToken);
        return Ok(orders);
    }

    [HttpPut("{orderId:guid}/status")]
    [Authorize(Roles = "RestaurantPartner,DeliveryAgent,Admin")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        request.OrderId = orderId;

        var role = this.GetCurrentUserRole();
        if (!this.CanRoleTransition(role, request.TargetStatus))
        {
            return Forbid();
        }

        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!this.CanAccessOrder(order))
        {
            return Forbid();
        }

        var updated = await _orderWorkflowService.UpdateOrderStatusAsync(request, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var role = this.GetCurrentUserRole();
        if (role == "Customer")
        {
            var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
            if (!this.CanAccessOrder(order))
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

    [HttpPost("{orderId:guid}/reorder")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ReorderFromHistory(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var originalOrder = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        if (!this.CanAccessOrder(originalOrder))
        {
            return Forbid();
        }

        var newOrder = await _orderWorkflowService.ReorderFromHistoryAsync(orderId, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = newOrder.OrderId }, newOrder);
    }

    [HttpGet("queue")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<IActionResult> GetOrderQueue(CancellationToken cancellationToken)
    {
        var orders = await _orderWorkflowService.GetOrderQueueAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("deliveries/assigned")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> GetAssignedDeliveries(
        [FromQuery] Guid deliveryAgentId,
        CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty || currentUserId != deliveryAgentId)
        {
            return Forbid();
        }

        var deliveries = await _orderWorkflowService.GetAssignedDeliveriesAsync(deliveryAgentId, cancellationToken);
        return Ok(deliveries);
    }
}

