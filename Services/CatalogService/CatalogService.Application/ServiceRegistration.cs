using AutoMapper;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Mappers;
using CatalogService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper with dependency injection
        services.AddAutoMapper(typeof(ServiceRegistration).Assembly);

        // Register service layer
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISearchService, SearchService>();

        return services;
    }
}
