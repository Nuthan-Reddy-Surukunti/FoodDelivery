namespace CatalogService.Application.DTOs.MenuItem;

using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Enums;

public class CreateMenuItemDto
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [Range(0, 999)]
    public int? PrepTime { get; set; }

    [Required]
    public bool IsVeg { get; set; }

    public string? ImageUrl { get; set; }
}
