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
    /// Get all restaurants (paginated) - active only by default, all for Admin
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userRole = this.GetCurrentUserRole();
            var result = await _restaurantService.GetAllRestaurantsAsync(pageNumber, pageSize, userRole);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing restaurant (Admin or RestaurantPartner - own restaurant only)
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
            return BadRequest(ex.Message);
        }
    }
}

