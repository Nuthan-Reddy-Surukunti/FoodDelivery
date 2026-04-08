namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Helpers;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    }

    public async Task<CartDto> GetOrCreateCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(restaurantId, nameof(restaurantId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId,
                RestaurantId = restaurantId,
                Status = CartStatus.Active,
                AppliedCouponCode = null,
                TotalAmount = 0,
                Items = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        return CartMappings.MapToDto(cart);
    }

    public async Task<CartDto> AddCartItemAsync(AddCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ServiceValidationHelper.ValidateIdentity(request.MenuItemId, nameof(request.MenuItemId));
        ServiceValidationHelper.ValidatePositive(request.Quantity, nameof(request.Quantity));
        ServiceValidationHelper.ValidatePositive(request.PriceSnapshot, nameof(request.PriceSnapshot));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        var isNewCart = cart is null;

        cart ??= new Cart
        {
            UserId = request.UserId,
            RestaurantId = request.RestaurantId,
            Status = CartStatus.Active,
            AppliedCouponCode = null,
            TotalAmount = 0,
            Items = new List<CartItem>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newItem = new CartItem
        {
            CartId = cart.Id,
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            Price = request.PriceSnapshot,
            CustomizationNotes = request.CustomizationNotes,
            Subtotal = request.Quantity * request.PriceSnapshot,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        cart.Items.Add(newItem);
        cart.TotalAmount += newItem.Subtotal;
        cart.UpdatedAt = DateTime.UtcNow;

        if (isNewCart)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return CartMappings.MapToDto(cart);
    }

    public async Task<CartDto> RemoveCartItemAsync(RemoveCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.CartItemId, nameof(request.CartItemId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", request.RestaurantId);
        }

        var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (itemToRemove is null)
        {
            throw new ResourceNotFoundException("CartItem", request.CartItemId);
        }

        cart.TotalAmount -= itemToRemove.Subtotal;
        cart.Items.Remove(itemToRemove);
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return CartMappings.MapToDto(cart);
    }

    public async Task<CartDto> ClearCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(restaurantId, nameof(restaurantId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", restaurantId);
        }

        cart.Items.Clear();
        cart.TotalAmount = 0;
        cart.AppliedCouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return CartMappings.MapToDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.CartItemId, nameof(request.CartItemId));
        ServiceValidationHelper.ValidatePositive(request.NewQuantity, nameof(request.NewQuantity));

        var cartItem = await _cartRepository.GetCartItemAsync(request.CartItemId, cancellationToken);
        if (cartItem is null)
        {
            throw new ResourceNotFoundException("CartItem", request.CartItemId);
        }

        var cart = await _cartRepository.GetByIdAsync(cartItem.CartId, cancellationToken);
        if (cart is null || cart.UserId != request.UserId)
        {
            throw new ResourceNotFoundException("Cart", cartItem.CartId);
        }

        cartItem = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (cartItem is null)
        {
            throw new ResourceNotFoundException("CartItem", request.CartItemId);
        }

        var oldSubtotal = cartItem.Subtotal;
        cartItem.Quantity = request.NewQuantity;
        cartItem.Subtotal = request.NewQuantity * cartItem.Price;
        cartItem.UpdatedAt = DateTime.UtcNow;

        cart.TotalAmount = cart.TotalAmount - oldSubtotal + cartItem.Subtotal;
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return CartMappings.MapToDto(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(ApplyCouponRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ServiceValidationHelper.ValidateNotNullOrWhitespace(request.CouponCode, nameof(request.CouponCode));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", request.RestaurantId);
        }

        if (!cart.Items.Any())
        {
            throw new ValidationException("Cannot apply coupon to an empty cart.");
        }

        cart.AppliedCouponCode = request.CouponCode.Trim();
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return CartMappings.MapToDto(cart);
    }

    public async Task<bool> ValidateCartItemsAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(restaurantId, nameof(restaurantId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
        {
            return false;
        }

        return true;
    }

    public async Task<PricingBreakdownDto> CalculateTotalsAsync(Guid userId, Guid restaurantId, decimal taxPercentage = 0, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        ServiceValidationHelper.ValidateIdentity(restaurantId, nameof(restaurantId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", restaurantId);
        }

        var subtotal = cart.Items.Sum(i => i.Subtotal);
        var taxAmount = subtotal * (taxPercentage / 100);
        var total = subtotal + taxAmount;

        return new PricingBreakdownDto
        {
            Subtotal = subtotal,
            Tax = taxAmount,
            TaxPercentage = taxPercentage,
            Discount = 0,
            Total = total,
            Currency = "INR"
        };
    }
}
