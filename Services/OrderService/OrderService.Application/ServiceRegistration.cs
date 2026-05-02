using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;

namespace OrderService.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Split services
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderPlacementService, OrderPlacementService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IUserAddressService, UserAddressService>();
        services.AddScoped<IDeliveryAgentSyncService, DeliveryAgentSyncService>();
        services.AddScoped<IProfileStatsService, ProfileStatsService>();

        return services;
    }
}