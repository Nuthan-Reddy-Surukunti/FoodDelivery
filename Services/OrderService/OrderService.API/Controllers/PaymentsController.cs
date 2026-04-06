using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/payments")]
[Authorize(Roles = "Customer")]
public class PaymentsController : ControllerBase
{
    private readonly IOrderWorkflowService _orderWorkflowService;

    public PaymentsController(IOrderWorkflowService orderWorkflowService)
    {
        _orderWorkflowService = orderWorkflowService;
    }

    [HttpPost("simulate")]
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

    [HttpPost("{orderId:guid}/process")]
    public async Task<IActionResult> ProcessPayment(
        [FromRoute] Guid orderId,
        [FromBody] ProcessPaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("OrderId is required");
        }

        var order = await _orderWorkflowService.GetOrderByIdAsync(orderId, cancellationToken);
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty || order.UserId != currentUserId)
        {
            return Forbid();
        }

        var paymentResponse = await _orderWorkflowService.ProcessPaymentAsync(orderId, request, cancellationToken);
        return Ok(paymentResponse);
    }
}
