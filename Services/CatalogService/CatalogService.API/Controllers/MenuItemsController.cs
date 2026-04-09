using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;
    private readonly IMapper _mapper;

    public MenuItemsController(IMenuItemService menuItemService, IMapper mapper)
    {
        _menuItemService = menuItemService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get menu item by ID - active restaurant and available item only
    /// </summary>
    [HttpGet("{id}")]
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
    /// Get all menu items for a restaurant (paginated) - active restaurant and available items only
    /// </summary>
    [HttpGet("/api/restaurants/{restaurantId}/menu")]
    public async Task<ActionResult> GetByRestaurant(
        [FromRoute] Guid restaurantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _menuItemService.GetMenuItemsByRestaurantAsync(restaurantId, pageNumber, pageSize, userRole, userId);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            return BadRequest(ex.Message);
        }
        catch (MenuItemNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (RestaurantNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
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
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
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
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }
}

public class AvailabilityUpdateDto
{
    public ItemAvailabilityStatus Status { get; set; }
}
