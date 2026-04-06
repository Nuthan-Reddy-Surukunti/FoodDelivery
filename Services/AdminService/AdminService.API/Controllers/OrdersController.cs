using Microsoft.AspNetCore.Mvc;
using AdminService.Application.Services;
using AdminService.Application.DTOs.Requests;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var result = await _orderService.GetAllAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("disputed")]
    public async Task<IActionResult> GetDisputedOrders()
    {
        var orders = await _orderService.GetDisputedOrdersAsync();
        return Ok(orders);
    }

    [HttpPost("{id}/resolve-dispute")]
    public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeRequest request)
    {
        try
        {
            var order = await _orderService.ResolveDisputeAsync(id, request);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
