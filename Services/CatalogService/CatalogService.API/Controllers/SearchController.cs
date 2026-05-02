using AutoMapper;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Enums;
using QuickBite.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, IMapper mapper, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _mapper = mapper;
        _logger = logger;
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
            _logger.LogError(ex, "Advanced restaurant search failed.");
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
            _logger.LogError(ex, "Restaurant name search failed.");
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
            _logger.LogError(ex, "Homepage data retrieval failed.");
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
    /// Universal search for menu items across all active restaurants
    /// </summary>
    [HttpGet("menuItems")]
    public async Task<ActionResult> SearchMenuItems([FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return BadRequest("Query must be at least 2 characters.");

        try
        {
            var result = await _searchService.SearchMenuItemsAsync(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Menu item search failed.");
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
}
