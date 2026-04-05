using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderWorkflowService _orderWorkflowService;

    public OrdersController(IOrderWorkflowService orderWorkflowService)
    {
        _orderWorkflowService = orderWorkflowService;
    }

    [HttpGet("cart")]
    public async Task<IActionResult> GetCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var cart = await _orderWorkflowService.GetOrCreateCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("cart/items")]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var cart = await _orderWorkflowService.AddCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpPut("cart/items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid cartItemId,
        [FromBody] UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        request.CartItemId = cartItemId;
        var cart = await _orderWorkflowService.UpdateCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete("cart/items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveCartItem(
        [FromRoute] Guid cartItemId,
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
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
    public async Task<IActionResult> ClearCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var cart = await _orderWorkflowService.ClearCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("cart/apply-coupon")]
    public async Task<IActionResult> ApplyCoupon(
        [FromBody] ApplyCouponRequestDto request,
        CancellationToken cancellationToken)
    {
        var cart = await _orderWorkflowService.ApplyCouponAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpGet("checkout/context")]
    public async Task<IActionResult> GetCheckoutContext(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var context = await _orderWorkflowService.GetCheckoutContextAsync(userId, cancellationToken);
        return Ok(context);
    }

    [HttpPost("checkout/validate")]
    public async Task<IActionResult> ValidateCheckout(
        [FromBody] CheckoutValidationRequestDto request,
        CancellationToken cancellationToken)
    {
        var isValid = await _orderWorkflowService.ValidateCheckoutAsync(request, cancellationToken);
        return Ok(new { isValid });
    }

    [HttpPost("payments/simulate")]
    public async Task<IActionResult> SimulatePayment(
        [FromBody] SimulatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.SimulatePaymentAsync(request, cancellationToken);
        return Ok(order);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.PlaceOrderAsync(request, cancellationToken);
        return Ok(order);
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<IActionResult> GetOrderById(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        var timeline = await _orderWorkflowService.GetOrderTimelineAsync(orderId, cancellationToken);

        return Ok(new
        {
            order,
            timeline
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrdersByUser(
        [FromQuery] Guid userId,
        [FromQuery] bool activeOnly,
        CancellationToken cancellationToken)
    {
        var orders = await _orderWorkflowService.GetOrdersByUserAsync(userId, activeOnly, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("orders/{orderId:guid}/timeline")]
    public async Task<IActionResult> GetOrderTimeline(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var timeline = await _orderWorkflowService.GetOrderTimelineAsync(orderId, cancellationToken);
        return Ok(timeline);
    }
}
