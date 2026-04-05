namespace OrderService.Domain.ValueObjects;

public sealed record CouponCode
{
    public string Code { get; }

    public decimal DiscountPercentage { get; }

    public Money MinOrderValue { get; }

    public DateTime ExpiryDateUtc { get; }

    public Guid? RestaurantId { get; }

    public CouponCode(
        string code,
        decimal discountPercentage,
        Money minOrderValue,
        DateTime expiryDateUtc,
        Guid? restaurantId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Coupon code is required.", nameof(code));
        }

        if (discountPercentage <= 0 || discountPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount percentage must be in range (0, 100].");
        }

        Code = code.Trim().ToUpperInvariant();
        DiscountPercentage = decimal.Round(discountPercentage, 2, MidpointRounding.AwayFromZero);
        MinOrderValue = minOrderValue;
        ExpiryDateUtc = expiryDateUtc;
        RestaurantId = restaurantId;
    }

    public bool IsExpired(DateTime atUtc)
    {
        return atUtc > ExpiryDateUtc;
    }

    public bool IsValid(DateTime atUtc)
    {
        return !IsExpired(atUtc);
    }

    public bool CanApplyTo(Money orderTotal, Guid restaurantId, DateTime atUtc)
    {
        if (!IsValid(atUtc))
        {
            return false;
        }

        if (!orderTotal.Currency.Equals(MinOrderValue.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (orderTotal.Amount < MinOrderValue.Amount)
        {
            return false;
        }

        return !RestaurantId.HasValue || RestaurantId == restaurantId;
    }

    public Money CalculateDiscount(Money orderTotal)
    {
        var discountFactor = DiscountPercentage / 100;
        return orderTotal.Multiply(discountFactor);
    }
}