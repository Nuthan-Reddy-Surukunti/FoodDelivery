namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.Helpers;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.Search;

public interface ISearchService
{
    Task<PaginatedResultDto<RestaurantDto>> SearchByNameAsync(string query, int pageNumber, int pageSize);

    Task<PaginatedResultDto<RestaurantDto>> AdvancedSearchAsync(SearchRestaurantFilterDto filters);

    Task<HomePageDto> GetHomepageDataAsync(string? serviceZoneId);
}
