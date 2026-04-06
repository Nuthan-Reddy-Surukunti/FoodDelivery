using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/carts")]
[Authorize(Roles = "Customer")]
public class CartsController : ControllerBase
{
    private readonly IOrderWorkflowService _orderWorkflowService;

    public CartsController(IOrderWorkflowService orderWorkflowService)
    {
        _orderWorkflowService = orderWorkflowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.GetOrCreateCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.AddCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid cartItemId,
        [FromBody] UpdateCartItemRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        request.CartItemId = cartItemId;
        var cart = await _orderWorkflowService.UpdateCartItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveCartItem(
        [FromRoute] Guid cartItemId,
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(userId))
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

    [HttpDelete]
    public async Task<IActionResult> ClearCart(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.ClearCartAsync(userId, restaurantId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("apply-coupon")]
    public async Task<IActionResult> ApplyCoupon(
        [FromBody] ApplyCouponRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var cart = await _orderWorkflowService.ApplyCouponAsync(request, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("validate-items")]
    public async Task<IActionResult> ValidateCartItems(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var isValid = await _orderWorkflowService.ValidateCartItemsAsync(userId, restaurantId, cancellationToken);
        return Ok(new { isValid });
    }

    [HttpPost("calculate-totals")]
    public async Task<IActionResult> CalculateTotals(
        [FromQuery] Guid userId,
        [FromQuery] Guid restaurantId,
        [FromQuery] decimal taxPercentage = 0,
        CancellationToken cancellationToken = default)
    {
        if (!this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var pricing = await _orderWorkflowService.CalculateTotalsAsync(userId, restaurantId, taxPercentage, cancellationToken);
        return Ok(pricing);
    }
}
