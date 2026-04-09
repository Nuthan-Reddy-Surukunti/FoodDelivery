namespace CatalogService.Application.DTOs.Restaurant;

using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Enums;

public class UpdateRestaurantDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    public CuisineType? CuisineType { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Range(0, 999999.99)]
    public decimal? MinOrderValue { get; set; }

    [Range(0, 999)]
    public int? DeliveryTime { get; set; }

    public RestaurantStatus? Status { get; set; }

    // Optional: Admin can reassign ownership via OwnerId field
    public Guid? OwnerId { get; set; }
}
