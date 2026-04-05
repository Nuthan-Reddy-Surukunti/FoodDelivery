namespace CatalogService.Domain.Entities;

using CatalogService.Domain.Common;
using CatalogService.Domain.Enums;

public class Restaurant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public CuisineType CuisineType { get; set; }

    public Guid? OwnerId { get; set; }

    public decimal Rating { get; set; } = 0;

    public decimal? MinOrderValue { get; set; }

    public int? DeliveryTime { get; set; }

    public RestaurantStatus Status { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = [];

    public ICollection<Category> Categories { get; set; } = [];

    public ICollection<OperatingHours> OperatingHours { get; set; } = [];
}
