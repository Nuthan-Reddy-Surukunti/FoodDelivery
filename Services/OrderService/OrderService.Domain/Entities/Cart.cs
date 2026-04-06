namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Constants;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = [];

    public Guid UserId { get; private set; }

    public Guid RestaurantId { get; private set; }

    public CartStatus Status { get; private set; } = CartStatus.Active;

    public CouponCode? AppliedCoupon { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
    }

    public Cart(Guid userId, Guid restaurantId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        if (restaurantId == Guid.Empty)
        {
            throw new ArgumentException("Restaurant ID is required.", nameof(restaurantId));
        }

        UserId = userId;
        RestaurantId = restaurantId;
    }

    public CartItem AddItem(
        Guid itemRestaurantId,
        Guid menuItemId,
        int quantity,
        decimal priceSnapshot,
        string? customizationNotes = null)
    {
        EnsureCartIsActive();

        if (IsMixedCart(itemRestaurantId))
        {
            throw new MixedCartException(RestaurantId, itemRestaurantId);
        }

        var normalizedNotes = string.IsNullOrWhiteSpace(customizationNotes) ? null : customizationNotes.Trim();
        var existingItem = _items.FirstOrDefault(item =>
            item.MenuItemId == menuItemId &&
            string.Equals(item.CustomizationNotes, normalizedNotes, StringComparison.Ordinal));

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            Touch();
            return existingItem;
        }

        var cartItem = new CartItem(Id, menuItemId, quantity, priceSnapshot, normalizedNotes);
        _items.Add(cartItem);
        Touch();
        return cartItem;
    }

    public void RemoveItem(Guid cartItemId)
    {
        var removed = _items.RemoveAll(item => item.Id == cartItemId) > 0;
        if (removed)
        {
            Touch();
        }
    }

    public void ClearCart()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items.Clear();
        Touch();
    }

    public Money CalculateTotal(string currency = DomainConstants.DefaultCurrency)
    {
        var total = _items.Sum(item => item.Subtotal);
        return new Money(total, currency);
    }

    public bool IsMixedCart(Guid itemRestaurantId)
    {
        return itemRestaurantId != RestaurantId;
    }

    public void ApplyCoupon(CouponCode couponCode)
    {
        EnsureCartIsActive();
        AppliedCoupon = couponCode ?? throw new ArgumentNullException(nameof(couponCode));
        Touch();
    }

    public void RemoveCoupon()
    {
        if (AppliedCoupon is not null)
        {
            AppliedCoupon = null;
            Touch();
        }
    }

    public CouponCode? GetAppliedCoupon()
    {
        return AppliedCoupon;
    }

    private void EnsureCartIsActive()
    {
        if (Status != CartStatus.Active)
        {
            throw new CartException("Only active carts can be modified.");
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}