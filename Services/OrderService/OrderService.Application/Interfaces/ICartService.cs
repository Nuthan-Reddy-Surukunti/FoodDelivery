namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Requests;

public interface ICartService
{
    Task<CartDto> GetOrCreateCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<CartDto> AddCartItemAsync(AddCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> RemoveCartItemAsync(RemoveCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> ClearCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> ApplyCouponAsync(ApplyCouponRequestDto request, CancellationToken cancellationToken = default);

    Task<bool> ValidateCartItemsAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<PricingBreakdownDto> CalculateTotalsAsync(Guid userId, Guid restaurantId, decimal taxPercentage = 0, CancellationToken cancellationToken = default);
}
