namespace OrderService.Application.DTOs.Cart;

using OrderService.Domain.Enums;

public class CartDto
{
    public Guid CartId { get; set; }

    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public CartStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CartItemDto> Items { get; set; } = [];

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";
}