namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Helpers;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

public class OrderPlacementService : IOrderPlacementService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;

    public OrderPlacementService(ICartRepository cartRepository, IOrderRepository orderRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public Task<CheckoutContextDto> GetCheckoutContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));

        var context = new CheckoutContextDto
        {
            Cart = new CartDto
            {
                CartId = Guid.Empty,
                UserId = userId,
                RestaurantId = Guid.Empty,
                Status = CartStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = [],
                TotalAmount = 0,
                Currency = "INR"
            },
            SavedAddresses = GetDefaultAddresses(),
            AvailableSlots = GetAvailableTimeSlots(),
            EstimatedDeliveryCharge = 50m,
            EstimatedDeliveryMinutes = 30
        };

        return Task.FromResult(context);
    }

    public async Task<bool> ValidateCheckoutAsync(CheckoutValidationRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ServiceValidationHelper.ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));
        ServiceValidationHelper.ValidateIdentity(request.SelectedTimeSlotId, nameof(request.SelectedTimeSlotId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
            throw new ValidationException("Cart is empty or not found");

        return true;
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ServiceValidationHelper.ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));
        ServiceValidationHelper.ValidateIdentity(request.SelectedTimeSlotId, nameof(request.SelectedTimeSlotId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken)
            ?? throw new ValidationException("Cart not found");

        if (!cart.Items.Any())
            throw new ValidationException("Cart is empty");

        var order = new Order
        {
            UserId = request.UserId,
            RestaurantId = request.RestaurantId,
            OrderStatus = OrderStatus.CheckoutStarted,
            TotalAmount = cart.TotalAmount,
            CheckoutStartedAt = DateTime.UtcNow,
            OrderItems = cart.Items.Select(item => new OrderItem
            {
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                Subtotal = item.Subtotal
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        cart.Status = CartStatus.Abandoned;
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return OrderMappings.MapToDto(order);
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdWithItemsAsync(orderId, cancellationToken)
            ?? throw new ValidationException("Order not found");
        return OrderMappings.MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrdersByUserAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        if (activeOnly)
            orders = orders.Where(o => o.OrderStatus != OrderStatus.Delivered && o.OrderStatus != OrderStatus.CancelRequestedByCustomer).ToList();
        return orders.Select(OrderMappings.MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrderQueueAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetActiveOrdersAsync(cancellationToken);
        var filtered = orders.Where(o => o.OrderStatus != OrderStatus.CancelRequestedByCustomer).ToList();
        return filtered.Select(OrderMappings.MapToDto).ToList().AsReadOnly();
    }

    public async Task<OrderDetailDto> ReorderFromHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var original = await _orderRepository.GetOrderByIdWithItemsAsync(orderId, cancellationToken);
        if (original is null) 
            throw new ValidationException("Order not found");

        return OrderMappings.MapToDto(original);
    }

    private static List<AddressDto> GetDefaultAddresses() => new()
    {
        new() { Street = "123 Main St", City = "Bengaluru", Pincode = "560001", Latitude = 37.7749, Longitude = -122.4194, AddressType = AddressType.Home },
        new() { Street = "456 Work Ave", City = "Bengaluru", Pincode = "560002", Latitude = 37.7900, Longitude = -122.4000, AddressType = AddressType.Work }
    };

    private static List<TimeSlotDto> GetAvailableTimeSlots() => new()
    {
        new() { Id = Guid.NewGuid(), Label = "ASAP", StartMinutesFromNow = 0, EndMinutesFromNow = 30, IsAvailable = true },
        new() { Id = Guid.NewGuid(), Label = "Afternoon", StartMinutesFromNow = 60, EndMinutesFromNow = 120, IsAvailable = true },
        new() { Id = Guid.NewGuid(), Label = "Evening", StartMinutesFromNow = 180, EndMinutesFromNow = 240, IsAvailable = true }
    };
}
