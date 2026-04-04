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

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public double? RadiusInKm { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
}
