namespace AdminService.Domain.ValueObjects;

/// <summary>
/// Represents metrics for a report as an immutable value object
/// </summary>
public sealed class ReportMetrics
{
    public int TotalOrders { get; }
    public Money TotalRevenue { get; }
    public int TotalCustomers { get; }
    public int TotalRestaurants { get; }
    public double AverageOrderValue { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private ReportMetrics(
        int totalOrders,
        Money totalRevenue,
        int totalCustomers,
        int totalRestaurants,
        double averageOrderValue,
        DateTime startDate,
        DateTime endDate)
    {
        TotalOrders = totalOrders;
        TotalRevenue = totalRevenue;
        TotalCustomers = totalCustomers;
        TotalRestaurants = totalRestaurants;
        AverageOrderValue = averageOrderValue;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Creates a new ReportMetrics instance with validation
    /// </summary>
    public static ReportMetrics Create(
        int totalOrders,
        Money totalRevenue,
        int totalCustomers,
        int totalRestaurants,
        double averageOrderValue,
        DateTime startDate,
        DateTime endDate)
    {
        if (totalOrders < 0)
            throw new ArgumentException("Total orders cannot be negative", nameof(totalOrders));

        if (totalCustomers < 0)
            throw new ArgumentException("Total customers cannot be negative", nameof(totalCustomers));

        if (totalRestaurants < 0)
            throw new ArgumentException("Total restaurants cannot be negative", nameof(totalRestaurants));

        if (averageOrderValue < 0)
            throw new ArgumentException("Average order value cannot be negative", nameof(averageOrderValue));

        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date");

        return new ReportMetrics(
            totalOrders,
            totalRevenue,
            totalCustomers,
            totalRestaurants,
            averageOrderValue,
            startDate,
            endDate);
    }

    /// <summary>
    /// Creates empty metrics for a date range
    /// </summary>
    public static ReportMetrics Empty(DateTime startDate, DateTime endDate, string currency)
    {
        return Create(0, Money.Zero(currency), 0, 0, 0, startDate, endDate);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ReportMetrics other)
            return false;

        return TotalOrders == other.TotalOrders &&
               TotalRevenue.Equals(other.TotalRevenue) &&
               TotalCustomers == other.TotalCustomers &&
               TotalRestaurants == other.TotalRestaurants &&
               Math.Abs(AverageOrderValue - other.AverageOrderValue) < 0.01 &&
               StartDate == other.StartDate &&
               EndDate == other.EndDate;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TotalOrders, TotalRevenue, TotalCustomers, TotalRestaurants, AverageOrderValue, StartDate, EndDate);
    }

    public override string ToString()
    {
        return $"Orders: {TotalOrders}, Revenue: {TotalRevenue}, Customers: {TotalCustomers}, Restaurants: {TotalRestaurants}, Avg Order: {AverageOrderValue:N2}, Period: {StartDate:d} to {EndDate:d}";
    }
}
