namespace CatalogService.Application.DTOs.Restaurant;

using CatalogService.Application.DTOs.Category;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.OperatingHours;
using CatalogService.Domain.Enums;

public class RestaurantDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string ServiceZoneId { get; set; } = string.Empty;

    public CuisineType CuisineType { get; set; }

    public decimal Rating { get; set; }

    public int? DeliveryTime { get; set; }

    public decimal? MinOrderValue { get; set; }

    public RestaurantStatus Status { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CategoryDto> Categories { get; set; } = [];

    public List<MenuItemDto> MenuItems { get; set; } = [];

    public List<OperatingHoursDto> OperatingHours { get; set; } = [];
}
