namespace AdminService.Application.DTOs.Requests;

/// <summary>
/// Request to create a new menu item
/// </summary>
public class CreateMenuItemRequest
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CategoryId { get; set; }
}