namespace OrderService.Domain.ValueObjects;

using System.Text.RegularExpressions;
using OrderService.Domain.Constants;
using OrderService.Domain.Enums;

public sealed record Address
{
    private static readonly Regex PincodeRegex = new("^[1-9][0-9]{5}$", RegexOptions.Compiled);

    public string Street { get; }

    public string City { get; }

    public string Pincode { get; }

    public double? Latitude { get; }

    public double? Longitude { get; }

    public AddressType AddressType { get; }

    public Address(
        string street,
        string city,
        string pincode,
        AddressType addressType,
        double? latitude = null,
        double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street is required.", nameof(street));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(pincode) || !PincodeRegex.IsMatch(pincode))
        {
            throw new ArgumentException("Pincode must be a valid 6-digit value.", nameof(pincode));
        }

        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }

        Street = street.Trim();
        City = city.Trim();
        Pincode = pincode.Trim();
        Latitude = latitude;
        Longitude = longitude;
        AddressType = addressType;
    }

    public bool IsServiceable(double restaurantLatitude, double restaurantLongitude, double serviceRadiusKm)
    {
        if (!Latitude.HasValue || !Longitude.HasValue)
        {
            return false;
        }

        if (serviceRadiusKm < DomainConstants.MinDeliveryAddressDistanceKm ||
            serviceRadiusKm > DomainConstants.MaxDeliveryAddressDistanceKm)
        {
            return false;
        }

        var distance = CalculateDistanceKm(
            Latitude.Value,
            Longitude.Value,
            restaurantLatitude,
            restaurantLongitude);

        return distance <= serviceRadiusKm;
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}