namespace OrderService.Application.DTOs.Requests;

public class AddCartItemRequestDto
{
    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceSnapshot { get; set; }

    public string? CustomizationNotes { get; set; }
}