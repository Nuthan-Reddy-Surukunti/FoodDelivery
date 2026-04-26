namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.Helpers;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.Search;

public interface ISearchService
{
    Task<List<RestaurantDto>> SearchByNameAsync(string query);

    Task<List<RestaurantDto>> AdvancedSearchAsync(SearchRestaurantFilterDto filters);

    Task<HomePageDto> GetHomepageDataAsync(string? serviceZoneId);

    Task<List<MenuItemSearchResultDto>> SearchMenuItemsAsync(string query);
}

