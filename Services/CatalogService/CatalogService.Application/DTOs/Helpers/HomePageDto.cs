namespace CatalogService.Application.DTOs.Helpers;

using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Domain.Enums;

public class HomePageDto
{
    public List<RestaurantDto> FeaturedRestaurants { get; set; } = [];

    public List<RestaurantDto> NearbyRestaurants { get; set; } = [];

    public List<CuisineType> PopularCuisines { get; set; } = [];

    public string? BannerMessage { get; set; }

    public string? PromoMessage { get; set; }
}
