using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Enums;
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
                
                // Deserialize with StringEnumConverter to handle both text and numeric enum values
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                };
                var menuItem = JsonConvert.DeserializeObject<MenuItemResponseDto>(jsonContent, settings);

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

                // Check availability - CatalogService returns availability status enum
                bool isAvailable = menuItem.AvailabilityStatus == ItemAvailabilityStatus.Available;
                if (!isAvailable)
                {
                    string statusName = menuItem.AvailabilityStatus switch
                    {
                        ItemAvailabilityStatus.OutOfStock => "Out of Stock",
                        ItemAvailabilityStatus.Discontinued => "Discontinued",
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
    public ItemAvailabilityStatus AvailabilityStatus { get; set; } = ItemAvailabilityStatus.Available;
    public string? CategoryName { get; set; }
}
