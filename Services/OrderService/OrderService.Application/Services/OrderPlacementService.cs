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

    public async Task<CheckoutContextDto> GetCheckoutContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(userId, nameof(userId));
        return new CheckoutContextDto
        {
            Cart = new CartDto(),
            SavedAddresses = GetDefaultAddresses(),
            AvailableSlots = GetAvailableTimeSlots(),
            EstimatedDeliveryCharge = 30,
            EstimatedDeliveryMinutes = 45
        };
    }

    public async Task<bool> ValidateCheckoutAsync(CheckoutValidationRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
            throw new ValidationException("Cart is empty or does not exist.");
        return true;
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
            throw new ValidationException("Cart is empty.");
        
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
                Subtotal = item.Subtotal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _orderRepository.AddAsync(order, cancellationToken);
        cart.Items.Clear();
        cart.TotalAmount = 0;
        cart.Status = CartStatus.Completed;
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        return OrderMappings.MapToDto(order);
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(orderId, nameof(orderId));
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return order is null ? throw new ResourceNotFoundException("Order", orderId) : OrderMappings.MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrdersByUserAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersByUserAsync(userId, cancellationToken);
        if (activeOnly)
            orders = orders.Where(o => o.OrderStatus != OrderStatus.Delivered && o.OrderStatus != OrderStatus.Cancelled).ToList();
        return orders.Select(o => OrderMappings.MapToDto(o)).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrderQueueAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.Pending, cancellationToken);
        var preparing = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.Preparing, cancellationToken);
        return orders.Concat(preparing).Select(o => OrderMappings.MapToDto(o)).ToList().AsReadOnly();
    }

    public async Task<OrderDetailDto> ReorderFromHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var prev = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (prev is null) throw new ResourceNotFoundException("Order", orderId);
        
        var newOrder = new Order
        {
            UserId = prev.UserId,
            RestaurantId = prev.RestaurantId,
            OrderStatus = OrderStatus.Pending,
            TotalAmount = prev.TotalAmount,
            OrderItems = prev.OrderItems.Select(item => new OrderItem
            {
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _orderRepository.AddAsync(newOrder, cancellationToken);
        return OrderMappings.MapToDto(newOrder);
    }

    private static List<AddressDto> GetDefaultAddresses() => new()
    {
        new() { Street = "123 Main St", City = "Bengaluru", Pincode = "560001", Latitude = 37.7749, Longitude = -122.4194, AddressType = 0 },
        new() { Street = "456 Work Ave", City = "Bengaluru", Pincode = "560002", Latitude = 37.7900, Longitude = -122.4000, AddressType = 1 }
    };

    private static List<TimeSlotDto> GetAvailableTimeSlots()
    {
        var slots = new List<TimeSlotDto>();
        for (int i = 1; i <= 6; i++)
        {
            var st = DateTime.UtcNow.AddMinutes(i * 30);
            slots.Add(new TimeSlotDto { Id = Guid.NewGuid(), StartTime = st, EndTime = st.AddMinutes(30), IsAvailable = true });
        }
        return slots;
    }
}
