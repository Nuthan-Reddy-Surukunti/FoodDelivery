namespace OrderService.Domain.Constants;

public static class DomainConstants
{
    public const int CartExpiryDays = 7;

    public const int CustomerCancellationWindowMinutes = 120;

    public const double MinDeliveryAddressDistanceKm = 0.5;

    public const double MaxDeliveryAddressDistanceKm = 50;

    public const string DefaultCurrency = "INR";
}