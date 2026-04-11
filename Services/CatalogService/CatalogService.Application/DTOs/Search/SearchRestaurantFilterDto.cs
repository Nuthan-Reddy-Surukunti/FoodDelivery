namespace CatalogService.Application.DTOs.Search;

using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Enums;

public class SearchRestaurantFilterDto
{
    public string? Query { get; set; }

    public CuisineType? CuisineType { get; set; }

    [Range(0, 5)]
    public decimal? MinRating { get; set; }

    [Range(0, 999)]
    public int? MaxDeliveryTime { get; set; }

    [Range(0, 999999)]
    public decimal? MinPrice { get; set; }

    [Range(0, 999999)]
    public decimal? MaxPrice { get; set; }

    public string? City { get; set; }

    public string? ServiceZoneId { get; set; }

    public string? SortBy { get; set; }
}
