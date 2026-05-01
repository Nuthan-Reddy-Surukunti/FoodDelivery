using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using Newtonsoft.Json;

namespace OrderService.Infrastructure.Services;

/// <summary>
/// Validates a restaurant's active status by calling CatalogService over HTTP.
/// </summary>
public class RestaurantValidationService : IRestaurantValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestaurantValidationService> _logger;

    public RestaurantValidationService(HttpClient httpClient, ILogger<RestaurantValidationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RestaurantValidationResult> ValidateRestaurantAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/restaurants/{restaurantId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var restaurant = JsonConvert.DeserializeObject<RestaurantResponseDto>(json);

                if (restaurant == null)
                {
                    return new RestaurantValidationResult
                    {
                        IsActive = false,
                        RestaurantId = restaurantId,
                        ErrorMessage = "Restaurant not found in catalog"
                    };
                }

                // Status is "Active" (string comparison, case-insensitive)
                bool isActive = string.Equals(restaurant.Status, "Active", StringComparison.OrdinalIgnoreCase);
                if (!isActive)
                {
                    _logger.LogInformation(
                        "Restaurant {RestaurantId} is currently {Status} — order validation will fail",
                        restaurantId, restaurant.Status);
                }

                return new RestaurantValidationResult
                {
                    IsActive = isActive,
                    RestaurantId = restaurantId,
                    RestaurantName = restaurant.Name,
                    RestaurantAddress = $"{restaurant.Address}, {restaurant.City}",
                    ErrorMessage = isActive ? null : $"Restaurant is currently {restaurant.Status}"
                };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new RestaurantValidationResult
                {
                    IsActive = false,
                    RestaurantId = restaurantId,
                    ErrorMessage = "Restaurant does not exist"
                };
            }
            else
            {
                _logger.LogError(
                    "CatalogService returned {StatusCode} for restaurant {RestaurantId}",
                    response.StatusCode, restaurantId);
                // Fail-open: if catalog is unreachable, don't block the order
                return new RestaurantValidationResult
                {
                    IsActive = true,
                    RestaurantId = restaurantId,
                    ErrorMessage = "Unable to verify restaurant status — proceeding"
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error validating restaurant {RestaurantId}", restaurantId);
            // Fail-open: catalog may be temporarily down
            return new RestaurantValidationResult
            {
                IsActive = true,
                RestaurantId = restaurantId,
                ErrorMessage = "Catalog service unavailable — proceeding"
            };
        }
    }
}

internal class RestaurantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
