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
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
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

        var cart = await _cartService.GetOrCreateCartAsync(userId, restaurantId, cancellationToken);
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

        try
        {
            var cart = await _cartService.AddCartItemAsync(request, cancellationToken);
            return Ok(cart);
        }
        catch (OrderService.Application.Exceptions.ValidationException ex)
        {
            return BadRequest(new { statusCode = 400, message = ex.Message, timestamp = DateTime.UtcNow });
        }
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
        var cart = await _cartService.UpdateCartItemAsync(request, cancellationToken);
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

        var cart = await _cartService.RemoveCartItemAsync(request, cancellationToken);
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

        var cart = await _cartService.ClearCartAsync(userId, restaurantId, cancellationToken);
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

        var cart = await _cartService.ApplyCouponAsync(request, cancellationToken);
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

        var isValid = await _cartService.ValidateCartItemsAsync(userId, restaurantId, cancellationToken);
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

        var pricing = await _cartService.CalculateTotalsAsync(userId, restaurantId, taxPercentage, cancellationToken);
        return Ok(pricing);
    }
}
