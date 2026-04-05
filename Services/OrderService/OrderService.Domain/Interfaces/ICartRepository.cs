namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserAndRestaurantAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);

    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task<CartItem?> GetCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);

    Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
}