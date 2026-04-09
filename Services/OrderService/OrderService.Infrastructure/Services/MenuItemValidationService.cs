using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

                // Check availability - CatalogService returns availability status enum (Available = 1)
                bool isAvailable = menuItem.AvailabilityStatus == 1; // 1 = Available
                if (!isAvailable)
                {
                    string statusName = menuItem.AvailabilityStatus switch
                    {
                        2 => "Out of Stock",
                        3 => "Discontinued",
                        _ => "Unavailable"
                    };
                    _logger.LogInformation(
                        "Menu item {MenuItemId} is currently {Status}",
                        menuItemId, statusName);
                    return new MenuItemValidationResult
                    {
                        IsValid = false,
                        MenuItemId = menuItemId,
                        ItemName = menuItem.Name,
                        IsAvailable = false,
                        ErrorMessage = $"This item is currently {statusName}"
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
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? PrepTime { get; set; }
    public bool IsVeg { get; set; }
    public string? ImageUrl { get; set; }
    public int AvailabilityStatus { get; set; } = 1; // 1 = Available, 2 = OutOfStock, 3 = Discontinued
    public string? CategoryName { get; set; }
}
