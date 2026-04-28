using QuickBite.Shared.Events.Order;
using MassTransit;
using OrderService.Application.DTOs.Cart;
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
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IDeliveryService _deliveryService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderPlacementService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IUserAddressRepository userAddressRepository,
        IDeliveryService deliveryService,
        IPublishEndpoint publishEndpoint)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userAddressRepository = userAddressRepository ?? throw new ArgumentNullException(nameof(userAddressRepository));
        _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ServiceValidationHelper.ValidateIdentity(request.UserId, nameof(request.UserId));
        ServiceValidationHelper.ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ServiceValidationHelper.ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));

        var selectedAddress = await _userAddressRepository.GetByIdAsync(request.SelectedAddressId, cancellationToken)
            ?? throw new ValidationException("Selected delivery address was not found");

        if (selectedAddress.UserId != request.UserId)
        {
            throw new ValidationException("Selected delivery address does not belong to the user");
        }

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
            DeliveryAddressLine1 = selectedAddress.AddressLine1,
            DeliveryAddressLine2 = selectedAddress.AddressLine2,
            DeliveryCity = selectedAddress.City,
            DeliveryPostalCode = selectedAddress.PostalCode,
            DeliveryLatitude = selectedAddress.Latitude,
            DeliveryLongitude = selectedAddress.Longitude,
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

        // Process payment immediately (COD - Cash on Delivery)
        try
        {
            var paymentRequest = new ProcessPaymentRequestDto
            {
                PaymentMethod = PaymentMethod.CashOnDelivery,
                Amount = order.TotalAmount
            };
            var paymentResponse = await _deliveryService.ProcessPaymentAsync(order.Id, paymentRequest, cancellationToken);
            order.PaymentId = paymentResponse.PaymentId;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log payment failure but continue - customer can retry payment later
            System.Diagnostics.Debug.WriteLine($"Payment processing failed for order {order.Id}: {ex.Message}");
        }

        cart.Status = CartStatus.Abandoned;
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        // Publish order placed event
        await _publishEndpoint.Publish(new OrderPlacedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            RestaurantName = "", // Will be populated by handler if needed
            TotalAmount = order.TotalAmount,
            DeliveryAddress = "", // From request if available
            Items = order.OrderItems.Select(oi => new OrderItemSnapshot
            {
                MenuItemId = oi.MenuItemId,
                MenuItemName = "",
                Quantity = oi.Quantity,
                PriceAtPurchase = oi.UnitPrice
            }).ToList()
        }, cancellationToken);

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

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrderQueueAsync(Guid? restaurantId = null, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetActiveOrdersAsync(cancellationToken);
        var filtered = orders.Where(o => o.OrderStatus != OrderStatus.CancelRequestedByCustomer);
        if (restaurantId.HasValue && restaurantId.Value != Guid.Empty)
        {
            filtered = filtered.Where(o => o.RestaurantId == restaurantId.Value);
        }
        return filtered.Select(OrderMappings.MapToDto).ToList().AsReadOnly();
    }

    public async Task<OrderDetailDto> ReorderFromHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var original = await _orderRepository.GetOrderByIdWithItemsAsync(orderId, cancellationToken);
        if (original is null) 
            throw new ValidationException("Order not found");

        return OrderMappings.MapToDto(original);
    }

    public async Task<PartnerStatsDto> GetPartnerStatsAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByRestaurantIdAsync(restaurantId, cancellationToken);
        var today = DateTime.UtcNow.Date;

        var pending = orders.Count(o => o.OrderStatus == OrderStatus.Paid || o.OrderStatus == OrderStatus.CheckoutStarted);
        var preparing = orders.Count(o => o.OrderStatus == OrderStatus.RestaurantAccepted || o.OrderStatus == OrderStatus.Preparing);
        var completed = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
        var cancelled = orders.Count(o => o.OrderStatus == OrderStatus.CancelRequestedByCustomer || o.OrderStatus == OrderStatus.Refunded);

        var completedOrders = orders.Where(o => o.OrderStatus == OrderStatus.Delivered).ToList();
        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var todayOrders = orders.Where(o => o.CreatedAt.Date == today).ToList();
        var todayRevenue = todayOrders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount);

        // Group by Date for the chart
        var dailyRevenue = completedOrders
            .GroupBy(o => o.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .TakeLast(7) // Last 7 days
            .ToDictionary(
                g => g.Key.ToString("yyyy-MM-dd"),
                g => g.Sum(o => o.TotalAmount)
            );

        return new PartnerStatsDto
        {
            TotalOrders = orders.Count,
            PendingOrders = pending,
            PreparingOrders = preparing,
            CompletedOrders = completed,
            CancelledOrders = cancelled,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            TodayOrders = todayOrders.Count,
            DailyRevenue = dailyRevenue
        };
    }
}
