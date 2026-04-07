using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;

namespace OrderService.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Order Workflow Service
        services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();

        // Phase 2 Services
        services.AddScoped<IOrderPlacementService, OrderPlacementService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();
        services.AddScoped<IDeliveryService, DeliveryService>();

        return services;
    }
}