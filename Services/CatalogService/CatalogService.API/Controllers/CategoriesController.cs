using AutoMapper;
using CatalogService.API.Utilities;
using CatalogService.Application.DTOs.Category;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoriesController(ICategoryService categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all categories for a restaurant (ordered by display order)
    /// </summary>
    [HttpGet("/api/restaurants/{restaurantId}/categories")]
    public async Task<ActionResult> GetByRestaurant([FromRoute] Guid restaurantId)
    {
        try
        {
            var result = await _categoryService.GetCategoriesByRestaurantAsync(restaurantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(result);
        }
        catch (CategoryNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
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
            return Conflict(ex.Message);
        }
        catch (RestaurantNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Delete a category (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
    }
}
