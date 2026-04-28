using AutoMapper;
using CatalogService.Application.DTOs.Helpers;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class SearchService : ISearchService
{
    private readonly IRestaurantRepository _repository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;

    public SearchService(IRestaurantRepository repository, IMenuItemRepository menuItemRepository, IMapper mapper)
    {
        _repository = repository;
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
    }

    public async Task<List<RestaurantDto>> SearchByNameAsync(string query)
    {
        var restaurants = await _repository.SearchByNameAsync(query);
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        return restaurantDtos;
    }

    public async Task<List<RestaurantDto>> AdvancedSearchAsync(SearchRestaurantFilterDto filters)
    {
        var restaurants = await _repository.GetFilteredAsync(
            Domain.Enums.RestaurantStatus.Active, 
            filters.Query, 
            filters.CuisineType?.ToString(), 
            filters.MinRating,
            filters.City,
            false // isVegetarianOnly
        );

        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        return restaurantDtos;
    }

    public async Task<HomePageDto> GetHomepageDataAsync(string? serviceZoneId = null)
    {
        // Get featured restaurants (top rated)
        var featuredRestaurants = await _repository.GetByRatingAsync(4.0m);
        
        // Filter by service zone if provided
        if (!string.IsNullOrEmpty(serviceZoneId))
        {
            featuredRestaurants = featuredRestaurants.Where(r => r.ServiceZoneId == serviceZoneId).ToList();
        }
        
        // Limit to 10 featured restaurants for display
        featuredRestaurants = featuredRestaurants.Take(10).ToList();
        
        var featuredDtos = _mapper.Map<List<RestaurantDto>>(featuredRestaurants);

        // Get popular cuisines
        var popularCuisines = new List<Domain.Enums.CuisineType>
        {
            Domain.Enums.CuisineType.Italian,
            Domain.Enums.CuisineType.Chinese,
            Domain.Enums.CuisineType.Indian,
            Domain.Enums.CuisineType.Mexican,
            Domain.Enums.CuisineType.American
        };

        return new HomePageDto
        {
            FeaturedRestaurants = featuredDtos,
            NearbyRestaurants = new List<RestaurantDto>(),
            PopularCuisines = popularCuisines,
            BannerMessage = "Welcome to QuickBite - Order from your favorite restaurants!",
            PromoMessage = "Get 20% off on your first order with code WELCOME20"
        };
    }

    public async Task<List<MenuItemSearchResultDto>> SearchMenuItemsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<MenuItemSearchResultDto>();

        var items = await _menuItemRepository.SearchAsync(query);

        return items.Select(item => new MenuItemSearchResultDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            IsVeg = item.IsVeg,
            ImageUrl = item.ImageUrl,
            CategoryName = item.Category?.Name,
            RestaurantId = item.RestaurantId,
            RestaurantName = item.Restaurant?.Name ?? string.Empty,
            CuisineType = item.Restaurant != null ? (int)item.Restaurant.CuisineType : null,
        }).ToList();
    }
}
