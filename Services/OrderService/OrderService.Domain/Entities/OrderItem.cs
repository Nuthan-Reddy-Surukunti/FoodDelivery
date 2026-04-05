namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid MenuItemId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPriceSnapshot { get; private set; }

    public string? CustomizationNotes { get; private set; }

    public decimal Subtotal => CalculateSubtotal();

    private OrderItem()
    {
    }

    public OrderItem(Guid orderId, Guid menuItemId, int quantity, decimal unitPriceSnapshot, string? customizationNotes = null)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        if (menuItemId == Guid.Empty)
        {
            throw new ArgumentException("Menu item ID is required.", nameof(menuItemId));
        }

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        if (unitPriceSnapshot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPriceSnapshot), "Unit price cannot be negative.");
        }

        OrderId = orderId;
        MenuItemId = menuItemId;
        Quantity = quantity;
        UnitPriceSnapshot = decimal.Round(unitPriceSnapshot, 2, MidpointRounding.AwayFromZero);
        CustomizationNotes = string.IsNullOrWhiteSpace(customizationNotes) ? null : customizationNotes.Trim();
    }

    public decimal CalculateSubtotal()
    {
        return decimal.Round(Quantity * UnitPriceSnapshot, 2, MidpointRounding.AwayFromZero);
    }
}