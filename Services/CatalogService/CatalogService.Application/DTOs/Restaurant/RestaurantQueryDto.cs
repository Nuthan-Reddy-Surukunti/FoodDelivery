namespace CatalogService.Application.DTOs.Restaurant;

public class RestaurantQueryDto
{
    public string? SearchTerm { get; set; }
    public string? Cuisine { get; set; }
    public bool? IsVegetarianOnly { get; set; }
}
