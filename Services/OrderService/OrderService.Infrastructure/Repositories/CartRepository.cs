using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly OrderDbContext _context;

    public CartRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetCartByUserAndRestaurantAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include("_items")
            .FirstOrDefaultAsync(
                cart => cart.UserId == userId &&
                        cart.RestaurantId == restaurantId &&
                        cart.Status == CartStatus.Active,
                cancellationToken);
    }

    public async Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include("_items")
            .FirstOrDefaultAsync(cart => cart.Id == cartId, cancellationToken);
    }

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include("_items")
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

        if (cart is null)
        {
            return;
        }

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CartItem?> GetCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(item => item.Id == cartItemId, cancellationToken);
    }

    public async Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(item => item.Id == cartItemId, cancellationToken);

        if (cartItem is null)
        {
            return;
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cartItems = await _context.CartItems
            .Where(item => item.CartId == cartId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            return;
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
