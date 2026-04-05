namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }

    public Guid MenuItemId { get; private set; }

    public int Quantity { get; private set; }

    public decimal PriceSnapshot { get; private set; }

    public string? CustomizationNotes { get; private set; }

    public decimal Subtotal => decimal.Round(Quantity * PriceSnapshot, 2, MidpointRounding.AwayFromZero);

    private CartItem()
    {
    }

    public CartItem(Guid cartId, Guid menuItemId, int quantity, decimal priceSnapshot, string? customizationNotes = null)
    {
        if (cartId == Guid.Empty)
        {
            throw new ArgumentException("Cart ID is required.", nameof(cartId));
        }

        if (menuItemId == Guid.Empty)
        {
            throw new ArgumentException("Menu item ID is required.", nameof(menuItemId));
        }

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        if (priceSnapshot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priceSnapshot), "Price cannot be negative.");
        }

        CartId = cartId;
        MenuItemId = menuItemId;
        Quantity = quantity;
        PriceSnapshot = decimal.Round(priceSnapshot, 2, MidpointRounding.AwayFromZero);
        CustomizationNotes = string.IsNullOrWhiteSpace(customizationNotes) ? null : customizationNotes.Trim();
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return Quantity >= 1 && PriceSnapshot >= 0;
    }
}