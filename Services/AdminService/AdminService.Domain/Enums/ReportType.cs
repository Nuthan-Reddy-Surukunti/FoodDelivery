namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the type of report to be generated
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Sales and revenue report
    /// </summary>
    Sales = 1,

    /// <summary>
    /// User activity and analytics report
    /// </summary>
    UserAnalytics = 2,

    /// <summary>
    /// Restaurant performance metrics
    /// </summary>
    RestaurantPerformance = 3,

    /// <summary>
    /// Order analytics and trends
    /// </summary>
    OrderAnalytics = 4,

    /// <summary>
    /// Revenue breakdown by category
    /// </summary>
    Revenue = 5,

    /// <summary>
    /// Custom report based on specific criteria
    /// </summary>
    Custom = 6
}
