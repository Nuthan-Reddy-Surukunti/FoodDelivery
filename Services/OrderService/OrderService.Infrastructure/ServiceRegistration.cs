using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Services;

namespace OrderService.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderDb")));

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IDeliveryAssignmentRepository, DeliveryAssignmentRepository>();
        services.AddScoped<IUserAddressRepository, UserAddressRepository>();

        // Add HttpClient for CatalogService validation
        services.AddHttpClient<IMenuItemValidationService, MenuItemValidationService>((sp, client) =>
        {
            var catalogServiceUrl = configuration["Services:CatalogService:Url"] ?? "http://localhost:5002";
            client.BaseAddress = new Uri(catalogServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
