namespace AdminService.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency as an immutable value object
/// </summary>
public sealed class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a new Money instance with validation
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code (e.g., USD, EUR, INR)", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a Money instance with zero amount
    /// </summary>
    public static Money Zero(string currency) => Create(0, currency);

    /// <summary>
    /// Adds two Money instances (must have same currency)
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another Money instance from this one (must have same currency)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the amount by a factor
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return Create(Amount * factor, Currency);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Money other)
            return false;

        return Amount == other.Amount && Currency == other.Currency;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }

    public static bool operator ==(Money? left, Money? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal right) => left.Multiply(right);
}
