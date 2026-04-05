using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly IMapper _mapper;

    public RestaurantsController(IRestaurantService restaurantService, IMapper mapper)
    {
        _restaurantService = restaurantService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all restaurants (paginated)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _restaurantService.GetAllRestaurantsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Get restaurant by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _restaurantService.GetRestaurantByIdAsync(id);
            return Ok(result);
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a new restaurant (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
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
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing restaurant (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a restaurant (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var userRole = this.GetCurrentUserRole();
            await _restaurantService.DeleteRestaurantAsync(id, userId, userRole);
            return NoContent();
        }
        catch (RestaurantNotFoundException)
        {
            return NotFound();
        }
    }
}
