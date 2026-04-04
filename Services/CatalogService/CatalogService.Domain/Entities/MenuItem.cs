namespace CatalogService.Domain.Entities;

using CatalogService.Domain.Common;
using CatalogService.Domain.Enums;

public class MenuItem : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? PrepTime { get; set; }

    public bool IsVeg { get; set; }

    public string? ImageUrl { get; set; }

    public ItemAvailabilityStatus AvailabilityStatus { get; set; }

    public Restaurant? Restaurant { get; set; }

    public Category? Category { get; set; }
}
