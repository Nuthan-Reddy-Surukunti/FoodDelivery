using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Application.Interfaces;
using AdminService.Application.DTOs.Requests;
using System.Security.Claims;

namespace AdminService.API.Controllers;

[ApiController]
[Route("api/catalog/menu-items")]
[Authorize(Roles = "Admin")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemsController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService ?? throw new ArgumentNullException(nameof(menuItemService));
    }

    /// <summary>
    /// Gets paginated list of menu items with optional filters for admin use
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMenuItems(
        [FromQuery] Guid? restaurantId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? approvalStatus = null)
    {
        try
        {
            var result = await _menuItemService.GetAllAsync(restaurantId, page, pageSize, status, approvalStatus);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific menu item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuItem(Guid id)
    {
        try
        {
            var menuItem = await _menuItemService.GetByIdAsync(id);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets menu items pending approval for moderation
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        var menuItems = await _menuItemService.GetPendingApprovalsAsync();
        return Ok(menuItems);
    }

    /// <summary>
    /// Gets menu items for a specific restaurant
    /// </summary>
    [HttpGet("by-restaurant/{restaurantId}")]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
    {
        try
        {
            var menuItems = await _menuItemService.GetByRestaurantIdAsync(restaurantId);
            return Ok(menuItems);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets active menu items for a specific restaurant (for ordering)
    /// </summary>
    [HttpGet("active/by-restaurant/{restaurantId}")]
    [AllowAnonymous] // Allow public access for ordering system
    public async Task<IActionResult> GetActiveByRestaurant(Guid restaurantId)
    {
        try
        {
            var menuItems = await _menuItemService.GetActiveByRestaurantIdAsync(restaurantId);
            return Ok(menuItems);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new menu item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        try
        {
            var menuItem = await _menuItemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing menu item
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request)
    {
        try
        {
            var menuItem = await _menuItemService.UpdateAsync(id, request);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Activates a menu item (makes it available for ordering)
    /// </summary>
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateMenuItem(Guid id)
    {
        try
        {
            var menuItem = await _menuItemService.ActivateAsync(id);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivates a menu item (makes it unavailable for ordering)
    /// </summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateMenuItem(Guid id)
    {
        try
        {
            var menuItem = await _menuItemService.DeactivateAsync(id);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Approves a menu item for content moderation (admin action)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveMenuItem(Guid id, [FromBody] ApproveMenuItemRequest request)
    {
        try
        {
            // Get the admin user ID from the JWT claims
            var approvedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                           User.FindFirst("sub")?.Value ??
                           User.Identity?.Name ??
                           "Unknown Admin";

            var menuItem = await _menuItemService.ApproveAsync(id, request, approvedBy);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rejects a menu item for content moderation (admin action)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectMenuItem(Guid id, [FromBody] RejectMenuItemRequest request)
    {
        try
        {
            // Get the admin user ID from the JWT claims
            var rejectedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                           User.FindFirst("sub")?.Value ??
                           User.Identity?.Name ??
                           "Unknown Admin";

            var menuItem = await _menuItemService.RejectAsync(id, request, rejectedBy);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}