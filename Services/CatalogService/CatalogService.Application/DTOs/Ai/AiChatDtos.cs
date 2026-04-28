using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.MenuItem;

namespace CatalogService.Application.DTOs.Ai;

public class AiChatRequestDto
{
    public List<AiMessageDto> Messages { get; set; } = new();
}

public class AiMessageDto
{
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Text { get; set; } = string.Empty;
}

public class AiChatResponseDto
{
    public string Text { get; set; } = string.Empty;
    public List<RestaurantDto> RecommendedRestaurants { get; set; } = new();
    public List<MenuItemSearchResultDto> RecommendedMenuItems { get; set; } = new();
}
