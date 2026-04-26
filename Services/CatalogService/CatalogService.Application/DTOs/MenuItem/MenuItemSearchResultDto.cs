namespace CatalogService.Application.DTOs.MenuItem;

using CatalogService.Domain.Enums;

public class MenuItemSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int? CuisineType { get; set; }
}
