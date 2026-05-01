namespace OrderService.Application.Interfaces;

/// <summary>
/// Validates a restaurant's availability by calling CatalogService over HTTP.
/// </summary>
public interface IRestaurantValidationService
{
    Task<RestaurantValidationResult> ValidateRestaurantAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default);
}

public class RestaurantValidationResult
{
    public bool IsActive { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
