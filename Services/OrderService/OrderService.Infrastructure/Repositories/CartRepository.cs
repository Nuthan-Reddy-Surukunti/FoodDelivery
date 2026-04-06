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
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(
                cart => cart.UserId == userId &&
                        cart.RestaurantId == restaurantId &&
                        cart.Status == CartStatus.Active,
                cancellationToken);
    }

    public async Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == cartId, cancellationToken);
    }

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        var persistedItemIds = await _context.CartItems
            .Where(item => item.CartId == cart.Id)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

        var persistedSet = persistedItemIds.ToHashSet();
        var currentItems = cart.Items.ToList();
        var currentItemIdSet = currentItems.Select(item => item.Id).ToHashSet();

        if (_context.Entry(cart).State == EntityState.Detached)
        {
            _context.Carts.Attach(cart);
        }

        _context.Entry(cart).State = EntityState.Modified;

        foreach (var item in currentItems)
        {
            var entry = _context.Entry(item);

            if (persistedSet.Contains(item.Id))
            {
                if (entry.State == EntityState.Detached)
                {
                    _context.CartItems.Attach(item);
                    entry = _context.Entry(item);
                }

                entry.State = EntityState.Modified;
            }
            else
            {
                if (entry.State == EntityState.Detached)
                {
                    _context.CartItems.Add(item);
                }
                else
                {
                    entry.State = EntityState.Added;
                }
            }
        }

        var removedItemIds = persistedItemIds
            .Where(id => !currentItemIdSet.Contains(id))
            .ToList();

        if (removedItemIds.Count > 0)
        {
            var removedItems = await _context.CartItems
                .Where(item => removedItemIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

            _context.CartItems.RemoveRange(removedItems);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
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

