using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace OrderService.Infrastructure.Services;

/// <summary>
/// Service for validating menu items against CatalogService via HTTP
/// </summary>
public class MenuItemValidationService : IMenuItemValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MenuItemValidationService> _logger;

    public MenuItemValidationService(HttpClient httpClient, ILogger<MenuItemValidationService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates menu item existence and availability by calling CatalogService
    /// </summary>
    public async Task<MenuItemValidationResult> ValidateMenuItemAsync(
        Guid restaurantId,
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Call CatalogService to get menu item details
            var url = $"/api/menuitems/{menuItemId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var menuItem = JsonConvert.DeserializeObject<MenuItemResponseDto>(jsonContent);

                if (menuItem == null)
                {
                    _logger.LogWarning(
                        "Menu item {MenuItemId} not found in catalog response",
                        menuItemId);
                    return new MenuItemValidationResult
                    {
                        IsValid = false,
                        MenuItemId = menuItemId,
                        ErrorMessage = "Menu item not found in catalog"
                    };
                }

                // Verify restaurant matches
                if (menuItem.RestaurantId != restaurantId)
                {
                    _logger.LogWarning(
                        "Menu item {MenuItemId} belongs to different restaurant {ActualRestaurant} vs requested {RequestedRestaurant}",
                        menuItemId, menuItem.RestaurantId, restaurantId);
                    return new MenuItemValidationResult
                    {
                        IsValid = false,
                        MenuItemId = menuItemId,
                        ErrorMessage = "Menu item does not belong to this restaurant"
                    };
                }

                // Check availability
                if (!menuItem.IsAvailable)
                {
                    _logger.LogInformation(
                        "Menu item {MenuItemId} is currently unavailable",
                        menuItemId);
                    return new MenuItemValidationResult
                    {
                        IsValid = false,
                        MenuItemId = menuItemId,
                        ItemName = menuItem.Name,
                        IsAvailable = false,
                        ErrorMessage = "This item is currently unavailable"
                    };
                }

                // Valid and available
                _logger.LogDebug(
                    "Menu item {MenuItemId} validated successfully. Price: {Price}",
                    menuItemId, menuItem.Price);
                return new MenuItemValidationResult
                {
                    IsValid = true,
                    MenuItemId = menuItemId,
                    CurrentPrice = menuItem.Price,
                    ItemName = menuItem.Name,
                    IsAvailable = true
                };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "Menu item {MenuItemId} returned 404 from CatalogService",
                    menuItemId);
                return new MenuItemValidationResult
                {
                    IsValid = false,
                    MenuItemId = menuItemId,
                    ErrorMessage = "Menu item does not exist"
                };
            }
            else
            {
                _logger.LogError(
                    "CatalogService returned error status {StatusCode} for menu item {MenuItemId}",
                    response.StatusCode, menuItemId);
                return new MenuItemValidationResult
                {
                    IsValid = false,
                    MenuItemId = menuItemId,
                    ErrorMessage = $"Catalog service error: {response.StatusCode}"
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP error validating menu item {MenuItemId}: {Message}",
                menuItemId, ex.Message);
            return new MenuItemValidationResult
            {
                IsValid = false,
                MenuItemId = menuItemId,
                ErrorMessage = "Unable to connect to catalog service"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error validating menu item {MenuItemId}: {Message}",
                menuItemId, ex.Message);
            return new MenuItemValidationResult
            {
                IsValid = false,
                MenuItemId = menuItemId,
                ErrorMessage = "Error validating menu item"
            };
        }
    }
}

/// <summary>
/// DTO for menu item response from CatalogService
/// </summary>
internal class MenuItemResponseDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}
