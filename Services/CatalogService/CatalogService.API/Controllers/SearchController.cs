using AutoMapper;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly IMapper _mapper;

    public SearchController(ISearchService searchService, IMapper mapper)
    {
        _searchService = searchService;
        _mapper = mapper;
    }

    /// <summary>
    /// Advanced search for restaurants with filters
    /// </summary>
    [HttpGet("restaurants")]
    public async Task<ActionResult> SearchRestaurants(
        [FromQuery] string? query,
        [FromQuery] int? cuisineType,
        [FromQuery] decimal? minRating,
        [FromQuery] int? maxDeliveryTime,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? city,
        [FromQuery] string? serviceZoneId,
        [FromQuery] string? sortBy = "rating")
    {
        try
        {
            var filterDto = new SearchRestaurantFilterDto
            {
                Query = query,
                CuisineType = cuisineType.HasValue ? (CuisineType)cuisineType.Value : null,
                MinRating = minRating,
                MaxDeliveryTime = maxDeliveryTime,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                City = city,
                ServiceZoneId = serviceZoneId,
                SortBy = sortBy
            };

            var result = await _searchService.AdvancedSearchAsync(filterDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Search restaurants by name
    /// </summary>
    [HttpGet("restaurantsByName")]
    public async Task<ActionResult> SearchRestaurantsByName(
        [FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        try
        {
            var result = await _searchService.SearchByNameAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Get homepage data (featured restaurants, popular cuisines, by service zone)
    /// </summary>
    [HttpGet("home")]
    public async Task<ActionResult> GetHomepageData(
        [FromQuery] string? serviceZoneId)
    {
        try
        {
            var result = await _searchService.GetHomepageDataAsync(serviceZoneId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
