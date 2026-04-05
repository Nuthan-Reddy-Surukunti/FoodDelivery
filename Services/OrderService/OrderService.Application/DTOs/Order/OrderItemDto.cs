namespace OrderService.Application.DTOs.Order;

public class OrderItemDto
{
    public Guid OrderItemId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPriceSnapshot { get; set; }

    public string? CustomizationNotes { get; set; }

    public decimal Subtotal { get; set; }
}