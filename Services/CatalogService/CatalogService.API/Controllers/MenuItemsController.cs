using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Enums;
using QuickBite.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;
    private readonly IMapper _mapper;
    private readonly ILogger<MenuItemsController> _logger;

    public MenuItemsController(IMenuItemService menuItemService, IMapper mapper, ILogger<MenuItemsController> logger)
    {
        _menuItemService = menuItemService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get menu item by ID - active restaurant and available item only
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuItemDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.GetMenuItemByIdAsync(id, userRole);
            return Ok(result);
        }
        catch (MenuItemNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get all menu items for a restaurant - active restaurant and available items only
    /// </summary>
    [HttpGet("/api/restaurants/{restaurantId}/menu")]
    public async Task<ActionResult> GetByRestaurant(
        [FromRoute] Guid restaurantId)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.GetMenuItemsByRestaurantAsync(restaurantId, userRole, userId);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve menu items for restaurant {RestaurantId}.", restaurantId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// Create a new menu item (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<MenuItemDto>> Create([FromBody] CreateMenuItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.CreateMenuItemAsync(dto, userId, userRole);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidMenuItemPriceException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            });
        }
        catch (MenuItemNotFoundException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "The menu item could not be processed.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            });
        }
        catch (RestaurantNotFoundException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "The restaurant could not be processed.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "FORBIDDEN"
            });
        }
    }

    /// <summary>
    /// Update an existing menu item (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<MenuItemDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateMenuItemDto dto)
    {
        if (dto.Id != id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.UpdateMenuItemAsync(dto, userId, userRole);
            return Ok(result);
        }
        catch (MenuItemNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidMenuItemPriceException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "FORBIDDEN"
            });
        }
    }

    /// <summary>
    /// Delete a menu item (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.DeleteMenuItemAsync(id, userId, userRole);
            if (!result)
                return NotFound();
            return NoContent();
        }
        catch (MenuItemNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "FORBIDDEN"
            });
        }
    }

    /// <summary>
    /// Toggle menu item availability (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<MenuItemDto>> UpdateAvailability(
        [FromRoute] Guid id,
        [FromBody] AvailabilityUpdateDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.ToggleAvailabilityAsync(id, dto.Status, userId, userRole);
            return Ok(result);
        }
        catch (MenuItemNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "FORBIDDEN"
            });
        }
    }
}

public class AvailabilityUpdateDto
{
    public ItemAvailabilityStatus Status { get; set; }
}
