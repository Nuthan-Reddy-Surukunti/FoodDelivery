namespace AdminService.Domain.ValueObjects;

/// <summary>
/// Represents a physical address as an immutable value object
/// </summary>
public sealed class Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    /// <summary>
    /// Creates a new Address instance with validation
    /// </summary>
    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(street, city, state, zipCode, country);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Address other)
            return false;

        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Country == other.Country;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, ZipCode, Country);
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State} {ZipCode}, {Country}";
    }

    public static bool operator ==(Address? left, Address? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
}
