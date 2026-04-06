using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Application.Interfaces;
using AdminService.Application.DTOs.Requests;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRestaurants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var result = await _restaurantService.GetAllAsync(page, pageSize, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRestaurant(Guid id)
    {
        try
        {
            var restaurant = await _restaurantService.GetByIdAsync(id);
            return Ok(restaurant);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var restaurants = await _restaurantService.GetPendingApprovalsAsync();
        return Ok(restaurants);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRestaurant(Guid id, [FromBody] ApproveRestaurantRequest request)
    {
        try
        {
            var restaurant = await _restaurantService.ApproveAsync(id, request);
            return Ok(restaurant);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRestaurant(Guid id, [FromBody] RejectRestaurantRequest request)
    {
        try
        {
            var restaurant = await _restaurantService.RejectAsync(id, request);
            return Ok(restaurant);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
