namespace CatalogService.Application.DTOs.Restaurant;

using CatalogService.Domain.Enums;

public class RestaurantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public CuisineType CuisineType { get; set; }

    public string City { get; set; } = string.Empty;

    public decimal Rating { get; set; }

    public int? DeliveryTime { get; set; }

    public decimal? MinOrderValue { get; set; }

    public RestaurantStatus Status { get; set; }

    public string? ImageUrl { get; set; }
}
