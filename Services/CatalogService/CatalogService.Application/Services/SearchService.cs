using AutoMapper;
using CatalogService.Application.DTOs.Helpers;
using CatalogService.Application.DTOs.Pagination;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class SearchService : ISearchService
{
    private readonly IRestaurantRepository _repository;
    private readonly IMapper _mapper;

    public SearchService(IRestaurantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<RestaurantDto>> SearchByNameAsync(string query, int pageNumber = 1, int pageSize = 10)
    {
        var (restaurants, totalCount) = await _repository.SearchByNameAsync(query, pageNumber, pageSize);
        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        
        return new PaginatedResultDto<RestaurantDto>
        {
            Data = restaurantDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResultDto<RestaurantDto>> AdvancedSearchAsync(SearchRestaurantFilterDto filters)
    {
        var (restaurants, totalCount) = await _repository.GetAllAsync(filters.PageNumber, filters.PageSize);
        
        // Apply filters
        if (!string.IsNullOrEmpty(filters.Query))
        {
            var (searchResults, _) = await _repository.SearchByNameAsync(filters.Query, filters.PageNumber, filters.PageSize);
            restaurants = searchResults;
        }

        if (filters.CuisineType.HasValue)
        {
            var (cuisineResults, _) = await _repository.GetByCuisineAsync(filters.CuisineType.Value, filters.PageNumber, filters.PageSize);
            restaurants = restaurants.Intersect(cuisineResults).ToList();
        }

        if (filters.City != null)
        {
            restaurants = restaurants.Where(r => r.City == filters.City).ToList();
        }

        if (filters.MinRating.HasValue)
        {
            var (ratingResults, _) = await _repository.GetByRatingAsync(filters.MinRating.Value, filters.PageNumber, filters.PageSize);
            restaurants = restaurants.Intersect(ratingResults).ToList();
        }

        var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurants);
        
        return new PaginatedResultDto<RestaurantDto>
        {
            Data = restaurantDtos,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = restaurants.Count
        };
    }

    public async Task<HomePageDto> GetHomepageDataAsync(string? serviceZoneId = null)
    {
        // Get featured restaurants (top rated, up to 10)
        var (featuredRestaurants, _) = await _repository.GetByRatingAsync(4.0m, 1, 10);
        
        // Filter by service zone if provided
        if (!string.IsNullOrEmpty(serviceZoneId))
        {
            featuredRestaurants = featuredRestaurants.Where(r => r.ServiceZoneId == serviceZoneId).ToList();
        }
        
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
            NearbyRestaurants = new List<RestaurantDto>(), // Service zone replaces location-based nearby
            PopularCuisines = popularCuisines,
            BannerMessage = "Welcome to Food Delivery - Order from your favorite restaurants!",
            PromoMessage = "Get 20% off on your first order with code WELCOME20"
        };
    }
}
