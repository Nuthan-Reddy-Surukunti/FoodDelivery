namespace OrderService.Application.Options;

/// <summary>
/// Configuration for the delivery agent earnings model.
/// Bound from appsettings "DeliverySettings" section.
/// </summary>
public class DeliverySettings
{
    /// <summary>
    /// Percentage of the order total paid to the agent per delivery (0–100).
    /// Default: 10 (10%).
    /// </summary>
    public decimal EarningsPercentage { get; set; } = 10m;

    /// <summary>
    /// Maximum earnings cap per delivery in INR.
    /// Default: 80.
    /// </summary>
    public decimal MaxEarningsCapINR { get; set; } = 80m;
}
