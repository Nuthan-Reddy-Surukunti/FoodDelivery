using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/checkout")]
[Authorize(Roles = "Customer")]
public class CheckoutController : ControllerBase
{
    private readonly IOrderPlacementService _orderPlacementService;

    public CheckoutController(IOrderPlacementService orderPlacementService)
    {
        _orderPlacementService = orderPlacementService;
    }

    [HttpGet("context")]
    public async Task<IActionResult> GetCheckoutContext(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(userId))
        {
            return Forbid();
        }

        var context = await _orderPlacementService.GetCheckoutContextAsync(userId, cancellationToken);
        return Ok(context);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCheckout(
        [FromBody] CheckoutValidationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!this.IsCurrentUser(request.UserId))
        {
            return Forbid();
        }

        var isValid = await _orderPlacementService.ValidateCheckoutAsync(request, cancellationToken);
        return Ok(new { isValid });
    }
}
