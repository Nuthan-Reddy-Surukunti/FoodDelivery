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
    public AiActionDto? Action { get; set; }
    public OrderStatusInfoDto? OrderStatus { get; set; }
}

/// <summary>
/// Instructs the frontend to perform an action (e.g. add item to cart, navigate to orders).
/// </summary>
public class AiActionDto
{
    /// <summary>One of: "add_to_cart", "navigate_to_orders"</summary>
    public string Type { get; set; } = string.Empty;
    public AddToCartPayloadDto? CartPayload { get; set; }
}

public class AddToCartPayloadDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string? ImageUrl { get; set; }
    public Guid RestaurantId { get; set; }
}

/// <summary>
/// Summarised order status for display in the chat widget.
/// </summary>
public class OrderStatusInfoDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public List<string> ItemNames { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; }
}
