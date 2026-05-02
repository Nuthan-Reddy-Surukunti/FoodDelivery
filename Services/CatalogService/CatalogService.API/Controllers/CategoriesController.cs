using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.Category;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Exceptions;
using QuickBite.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, IMapper mapper, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories for a restaurant (ordered by display order) - active restaurant only
    /// </summary>
    [HttpGet("/api/restaurants/{restaurantId}/categories")]
    public async Task<ActionResult> GetByRestaurant([FromRoute] Guid restaurantId)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _categoryService.GetCategoriesByRestaurantAsync(restaurantId, userRole, userId);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve categories for restaurant {RestaurantId}.", restaurantId);
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
    /// Get category by ID - active restaurant only
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var userRole = this.GetCurrentUserRole();
            var result = await _categoryService.GetCategoryByIdAsync(id, userRole);
            return Ok(result);
        }
        catch (CategoryNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a new category (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _categoryService.CreateCategoryAsync(dto, userId, userRole);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (DuplicateCategoryException ex)
        {
            return Conflict(new ApiErrorResponse
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "CONFLICT"
            });
        }
        catch (RestaurantNotFoundException ex)
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
    /// Update an existing category (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<CategoryDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryDto dto)
    {
        if (dto.Id != id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _categoryService.UpdateCategoryAsync(dto, userId, userRole);
            return Ok(result);
        }
        catch (CategoryNotFoundException)
        {
            return NotFound();
        }
        catch (DuplicateCategoryException ex)
        {
            return Conflict(new ApiErrorResponse
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "CONFLICT"
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
    /// Delete a category (Admin or RestaurantPartner - own restaurant only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            await _categoryService.DeleteCategoryAsync(id, userId, userRole);
            return NoContent();
        }
        catch (CategoryNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidRestaurantDataException ex)
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
}
