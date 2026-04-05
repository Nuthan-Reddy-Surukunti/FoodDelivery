namespace OrderService.Application.DTOs.Cart;

public class CartItemDto
{
    public Guid CartItemId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceSnapshot { get; set; }

    public string? CustomizationNotes { get; set; }

    public decimal Subtotal { get; set; }
}