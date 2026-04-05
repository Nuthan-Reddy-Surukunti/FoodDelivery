using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;

namespace OrderService.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();
        return services;
    }
}