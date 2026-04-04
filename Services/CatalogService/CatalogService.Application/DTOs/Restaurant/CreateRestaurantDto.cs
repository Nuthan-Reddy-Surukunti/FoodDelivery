namespace CatalogService.Application.DTOs.Restaurant;

using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Enums;

public class CreateRestaurantDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    public CuisineType CuisineType { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Range(0, 999999.99)]
    public decimal? MinOrderValue { get; set; }

    [Range(0, 999)]
    public int? DeliveryTime { get; set; }
}
