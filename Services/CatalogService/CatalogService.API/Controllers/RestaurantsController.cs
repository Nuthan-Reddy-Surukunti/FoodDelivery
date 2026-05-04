using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.Restaurant;
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
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly IMapper _mapper;
    private readonly ILogger<RestaurantsController> _logger;

    public RestaurantsController(IRestaurantService restaurantService, IMapper mapper, ILogger<RestaurantsController> logger)
    {
        _restaurantService = restaurantService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all restaurants - active only by default, all for Admin
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Allow unauthenticated access to view restaurants (if applicable)
    public async Task<ActionResult> GetAll([FromQuery] RestaurantQueryDto query)
    {
        try
        {
            // Allow anonymous access if userRole is null
            string? userRole = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userRole = this.GetCurrentUserRole();
            }

            var result = await _restaurantService.GetAllRestaurantsAsync(query, userRole);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve restaurants.");
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
    /// Get restaurant by ID - active only by default, all for Admin
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var userRole = this.GetCurrentUserRole();
            var result = await _restaurantService.GetRestaurantByIdAsync(id, userRole);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get current restaurant partner's own restaurant (any status: pending/active/rejected)
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "RestaurantPartner")]
    public async Task<ActionResult<RestaurantDetailDto>> GetMyRestaurant()
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _restaurantService.GetRestaurantByOwnerAsync(userId, userRole);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
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
    /// Create a new restaurant (Admin or RestaurantPartner)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<RestaurantDetailDto>> Create([FromBody] CreateRestaurantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _restaurantService.CreateRestaurantAsync(dto, userId, userRole);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
    }

    /// <summary>
    /// Update an existing restaurant (Admin or RestaurantPartner - own restaurant only)
    /// 
    /// RestaurantPartner: Once admin approves your restaurant, you can freely edit details without requiring re-approval.
    /// You cannot change: restaurant ownership or status.
    /// 
    /// Admin: Can update any field including status and ownership.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RestaurantPartner")]
    public async Task<ActionResult<RestaurantDetailDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateRestaurantDto dto)
    {
        if (dto.Id != id)
            return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            var result = await _restaurantService.UpdateRestaurantAsync(id, dto, userId, userRole);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
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
    }
}
