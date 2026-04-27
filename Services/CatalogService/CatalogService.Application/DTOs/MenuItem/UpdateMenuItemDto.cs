namespace CatalogService.Application.DTOs.MenuItem;

using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Enums;

public class UpdateMenuItemDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0.01, 999999.99)]
    public decimal? Price { get; set; }

    [Range(0, 999)]
    public int? PrepTime { get; set; }

    public bool? IsVeg { get; set; }

    public string? ImageUrl { get; set; }

    public ItemAvailabilityStatus? AvailabilityStatus { get; set; }

    public Guid? CategoryId { get; set; }
}
