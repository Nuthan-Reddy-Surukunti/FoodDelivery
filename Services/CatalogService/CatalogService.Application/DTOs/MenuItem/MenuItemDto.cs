namespace CatalogService.Application.DTOs.MenuItem;

using CatalogService.Domain.Enums;

public class MenuItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? PrepTime { get; set; }

    public bool IsVeg { get; set; }

    public string? ImageUrl { get; set; }

    public ItemAvailabilityStatus AvailabilityStatus { get; set; }

    public string? CategoryName { get; set; }
}
