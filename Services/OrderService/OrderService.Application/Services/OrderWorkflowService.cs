namespace OrderService.Application.Services;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Domain.Common;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Interfaces;
using OrderService.Domain.ValueObjects;

public class OrderWorkflowService : IOrderWorkflowService
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryAssignmentRepository _deliveryAssignmentRepository;

    public OrderWorkflowService(
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IDeliveryAssignmentRepository deliveryAssignmentRepository)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _deliveryAssignmentRepository = deliveryAssignmentRepository;
    }

    public async Task<CartDto> GetOrCreateCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(userId, nameof(userId));
        ValidateIdentity(restaurantId, nameof(restaurantId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null)
        {
            cart = new Cart(userId, restaurantId);
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        return MapCart(cart);
    }

    public async Task<CartDto> AddCartItemAsync(AddCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ValidateIdentity(request.MenuItemId, nameof(request.MenuItemId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        var isNewCart = cart is null;
        cart ??= new Cart(request.UserId, request.RestaurantId);

        cart.AddItem(
            request.RestaurantId,
            request.MenuItemId,
            request.Quantity,
            request.PriceSnapshot,
            request.CustomizationNotes);

        if (isNewCart)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }

        return MapCart(cart);
    }

    public async Task<CartDto> RemoveCartItemAsync(RemoveCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", request.RestaurantId);
        }

        cart.RemoveItem(request.CartItemId);
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapCart(cart);
    }

    public async Task<CartDto> ClearCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(userId, restaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", restaurantId);
        }

        cart.ClearCart();
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapCart(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.CartItemId, nameof(request.CartItemId));

        if (request.NewQuantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0. Use RemoveCartItemAsync to remove items.");
        }

        var cartItem = await _cartRepository.GetCartItemAsync(request.CartItemId, cancellationToken)
                      ?? throw new ResourceNotFoundException("CartItem", request.CartItemId);

        var cart = await _cartRepository.GetByIdAsync(cartItem.CartId, cancellationToken)
                   ?? throw new ResourceNotFoundException("Cart", cartItem.CartId);

        if (cart.UserId != request.UserId)
        {
            throw new ValidationException("Cart item does not belong to the requested user.");
        }

        var existingCartItem = cart.Items.FirstOrDefault(item => item.Id == request.CartItemId)
                              ?? throw new ResourceNotFoundException("CartItem", request.CartItemId);

        existingCartItem.UpdateQuantity(request.NewQuantity);
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapCart(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(ApplyCouponRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));

        if (string.IsNullOrWhiteSpace(request.CouponCode))
        {
            throw new ValidationException("Coupon code is required.");
        }

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", request.RestaurantId);
        }

        if (!cart.Items.Any())
        {
            throw new CartEmptyException(cart.Id);
        }

        // Validate coupon code exists and is applicable
        // This would typically call a CouponService from external service
        // For now, create a default valid coupon (requires discount %, min order value, and expiry date)
        var couponCode = new CouponCode(
            request.CouponCode.Trim(),
            discountPercentage: 10,
            minOrderValue: new Money(100),
            expiryDateUtc: DateTime.UtcNow.AddDays(30),
            restaurantId: cart.RestaurantId);

        // In a real implementation, verify coupon validity from external service
        // For now, just store it in the domain entity
        cart.ApplyCoupon(couponCode);
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapCart(cart);
    }

    public async Task<CheckoutContextDto> GetCheckoutContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(userId, nameof(userId));

        // In a real app, this would fetch saved addresses from UserService
        var savedAddresses = new List<AddressDto>
        {
            new()
            {
                Street = "123 Main St",
                City = "San Francisco",
                Pincode = "94105",
                AddressType = AddressType.Home,
                Latitude = 37.7749,
                Longitude = -122.4194
            },
            new()
            {
                Street = "456 Work Ave",
                City = "San Francisco",
                Pincode = "94105",
                AddressType = AddressType.Work,
                Latitude = 37.7900,
                Longitude = -122.4000
            }
        };

        var availableSlots = GetAvailableTimeSlots();

        return new CheckoutContextDto
        {
            Cart = new CartDto(), // Would be populated with actual cart if one exists
            SavedAddresses = savedAddresses,
            AvailableSlots = availableSlots,
            EstimatedDeliveryCharge = 30,
            EstimatedDeliveryMinutes = 45
        };
    }

    public async Task<bool> ValidateCheckoutAsync(CheckoutValidationRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));
        ValidateIdentity(request.SelectedTimeSlotId, nameof(request.SelectedTimeSlotId));

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
        {
            throw new ValidationException("Cart is empty or does not exist.");
        }

        // Validate address exists and is valid
        // In a real app, this would fetch from UserService to verify address belongs to user
        // throw new ValidationException($"Address {request.SelectedAddressId} not found or not accessible to user.");

        // Validate time slot exists and is available
        var validSlots = GetAvailableTimeSlots();
        var selectedSlot = validSlots.FirstOrDefault(s => s.Id == request.SelectedTimeSlotId);
        if (selectedSlot is null)
        {
            throw new ValidationException($"Time slot {request.SelectedTimeSlotId} is not available.");
        }

        // Validate restaurant is still accepting orders
        // In a real app, this would call RestaurantService
        // if (!restaurantIsActive) throw new ValidationException("Restaurant is not accepting orders.");

        // All validations passed
        return true;
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));
        ValidateIdentity(request.SelectedTimeSlotId, nameof(request.SelectedTimeSlotId));

        // Validate checkout first
        await ValidateCheckoutAsync(
            new CheckoutValidationRequestDto
            {
                UserId = request.UserId,
                RestaurantId = request.RestaurantId,
                SelectedAddressId = request.SelectedAddressId,
                SelectedTimeSlotId = request.SelectedTimeSlotId,
                SpecialInstructions = request.SpecialInstructions
            },
            cancellationToken);

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null || !cart.Items.Any())
        {
            throw new ValidationException("Cart is empty or does not exist.");
        }

        var order = new Order(request.UserId, request.RestaurantId);
        foreach (var cartItem in cart.Items)
        {
            order.AddItem(cartItem.MenuItemId, cartItem.Quantity, cartItem.PriceSnapshot, cartItem.CustomizationNotes);
        }

        // In a real app, get actual address details from UserService/AddressService
        var address = new Address(
            "123 Main St",
            "San Francisco",
            "94105",
            AddressType.Home,
            37.7749,
            -122.4194);

        var now = DateTime.UtcNow;
        order.StartCheckout(address, now);
        order.MarkPaymentPending(now);

        await _orderRepository.AddAsync(order, cancellationToken);

        // Clear cart after order creation
        cart.ClearCart();
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapOrder(order);
    }

    public async Task<OrderDetailDto> CheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        if (cart is null)
        {
            throw new ResourceNotFoundException("Cart", request.RestaurantId);
        }

        if (!cart.Items.Any())
        {
            throw new CartEmptyException(cart.Id);
        }

        var order = new Order(request.UserId, request.RestaurantId);
        foreach (var cartItem in cart.Items)
        {
            order.AddItem(cartItem.MenuItemId, cartItem.Quantity, cartItem.PriceSnapshot, cartItem.CustomizationNotes);
        }

        var address = new Address(
            request.DeliveryAddress.Street,
            request.DeliveryAddress.City,
            request.DeliveryAddress.Pincode,
            request.DeliveryAddress.AddressType,
            request.DeliveryAddress.Latitude,
            request.DeliveryAddress.Longitude);

        var now = DateTime.UtcNow;
        order.StartCheckout(address, now);
        order.MarkPaymentPending(now);

        await _orderRepository.AddAsync(order, cancellationToken);

        cart.ClearCart();
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapOrder(order);
    }

    public async Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdWithItemsAsync(request.OrderId, cancellationToken)
                    ?? await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", request.OrderId);

        var amount = request.Amount.HasValue && request.Amount.Value > 0
            ? new Money(request.Amount.Value)
            : order.CalculateTotal(request.TaxPercentage);

        var isNewPayment = order.Payment is null;
        var payment = order.Payment ?? new Payment(order.Id, amount, request.PaymentMethod);

        var now = DateTime.UtcNow;
        if (request.IsSuccessful)
        {
            var transactionId = string.IsNullOrWhiteSpace(request.TransactionId)
                ? $"SIM-{Guid.NewGuid():N}"[..12]
                : request.TransactionId.Trim();

            payment.MarkAsSuccess(transactionId, now);
            order.AttachPayment(payment);
            order.MarkPaid(now);

            if (request.AutoAcceptByRestaurant)
            {
                order.AcceptByRestaurant(now.AddMinutes(1));
            }
        }
        else
        {
            payment.MarkAsFailed(request.FailureReason ?? "Simulated payment failure.", now);
            order.AttachPayment(payment);
            order.MoveToNextStatus(OrderStatus.PaymentFailed, now);
        }

        if (isNewPayment)
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
        else
        {
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapOrder(order);
    }

    public async Task<OrderDetailDto> AssignDeliveryAsync(AssignDeliveryRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", request.OrderId);

        var assignment = new DeliveryAssignment(order.Id, request.DeliveryAgentId, DateTime.UtcNow);
        order.AssignDelivery(assignment);

        await _deliveryAssignmentRepository.AddAsync(assignment, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return MapOrder(order);
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", request.OrderId);

        var now = DateTime.UtcNow;
        switch (request.TargetStatus)
        {
            case OrderStatus.RestaurantAccepted:
                order.AcceptByRestaurant(now);
                break;
            case OrderStatus.RestaurantRejected:
                order.RejectByRestaurant(now);
                break;
            case OrderStatus.Preparing:
                order.StartPreparing(now);
                break;
            case OrderStatus.ReadyForPickup:
                order.MarkReadyForPickup(now);
                break;
            case OrderStatus.PickedUp:
                order.MarkPickedUp(now);
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.MarkAsPickedUp(now);
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.OutForDelivery:
                order.MarkOutForDelivery(now);
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.MarkAsInTransit(now);
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.Delivered:
                order.MarkDelivered(now);
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.MarkAsDelivered(now);
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.CancelRequestedByCustomer:
                order.RequestCancellation(now);
                break;
            case OrderStatus.RefundInitiated:
                InitiateOrderRefund(order, request.RefundAmount, now);
                if (order.Payment is not null)
                {
                    await _paymentRepository.UpdateAsync(order.Payment, cancellationToken);
                }
                break;
            case OrderStatus.Refunded:
                CompleteOrderRefund(order, now);
                if (order.Payment is not null)
                {
                    await _paymentRepository.UpdateAsync(order.Payment, cancellationToken);
                }
                break;
            default:
                throw new ValidationException($"Unsupported status update target: {request.TargetStatus}");
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        return MapOrder(order);
    }

    public async Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdWithItemsAsync(orderId, cancellationToken)
                    ?? await _orderRepository.GetByIdAsync(orderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", orderId);

        return MapOrder(order);
    }

    public async Task<IReadOnlyList<OrderDetailDto>> GetOrdersByUserAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, cancellationToken);
        if (activeOnly)
        {
            orders = orders
                .Where(order => order.OrderStatus is not OrderStatus.Delivered and not OrderStatus.Refunded)
                .ToList();
        }

        return orders.Select(MapOrder).ToList();
    }

    public async Task<IReadOnlyList<OrderTimelineEntryDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", orderId);

        var timeline = new List<OrderTimelineEntryDto>
        {
            new() { Status = OrderStatus.DraftCart, OccurredAt = order.CreatedAt, Label = "Order draft created" }
        };

        AddTimelineIfPresent(timeline, OrderStatus.CheckoutStarted, order.CheckoutStartedAt, "Checkout started");
        AddTimelineIfPresent(timeline, OrderStatus.Paid, order.PaymentCompletedAt, "Payment completed");
        AddTimelineIfPresent(timeline, OrderStatus.Preparing, order.PreparationStartTime, "Preparation started");
        AddTimelineIfPresent(timeline, OrderStatus.PickedUp, order.PickupTime, "Picked up by delivery agent");
        AddTimelineIfPresent(timeline, OrderStatus.Delivered, order.DeliveryTime, "Delivered to customer");
        AddTimelineIfPresent(timeline, OrderStatus.CancelRequestedByCustomer, order.CancelRequestedAt, "Cancellation requested");

        return timeline.OrderBy(item => item.OccurredAt).ToList();
    }

    private static List<TimeSlotDto> GetAvailableTimeSlots()
    {
        return new()
        {
            new TimeSlotDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Label = "30 mins delivery",
                StartMinutesFromNow = 20,
                EndMinutesFromNow = 35,
                IsAvailable = true
            },
            new TimeSlotDto
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Label = "1 hour delivery",
                StartMinutesFromNow = 50,
                EndMinutesFromNow = 65,
                IsAvailable = true
            },
            new TimeSlotDto
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Label = "1.5 hours delivery",
                StartMinutesFromNow = 80,
                EndMinutesFromNow = 100,
                IsAvailable = true
            },
            new TimeSlotDto
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Label = "2 hours delivery",
                StartMinutesFromNow = 110,
                EndMinutesFromNow = 130,
                IsAvailable = true
            }
        };
    }

    private static void ValidateIdentity(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException($"{name} is required.");
        }
    }

    private static void AddTimelineIfPresent(
        List<OrderTimelineEntryDto> timeline,
        OrderStatus status,
        DateTime? timestamp,
        string label)
    {
        if (timestamp.HasValue)
        {
            timeline.Add(new OrderTimelineEntryDto
            {
                Status = status,
                OccurredAt = timestamp.Value,
                Label = label
            });
        }
    }

    private static void InitiateOrderRefund(Order order, decimal? refundAmount, DateTime now)
    {
        if (order.Payment is null)
        {
            throw new ValidationException("Payment must exist before refund initiation.");
        }

        var amount = refundAmount.HasValue
            ? new Money(refundAmount.Value, order.Payment.Amount.Currency)
            : order.Payment.Amount;

        order.Payment.InitiateRefund(amount, now);
        order.InitiateRefund(now);
    }

    private static void CompleteOrderRefund(Order order, DateTime now)
    {
        if (order.Payment is null)
        {
            throw new ValidationException("Payment must exist before refund completion.");
        }

        if (order.Payment.PaymentStatus != PaymentStatus.RefundInitiated)
        {
            order.Payment.InitiateRefund(order.Payment.Amount, now);
            order.InitiateRefund(now);
        }

        order.Payment.CompleteRefund(now);
        order.MarkRefunded(now);
    }

    private static CartDto MapCart(Cart cart)
    {
        var total = cart.CalculateTotal();
        return new CartDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            RestaurantId = cart.RestaurantId,
            Status = cart.Status,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            TotalAmount = total.Amount,
            Currency = total.Currency,
            Items = cart.Items.Select(item => new CartItemDto
            {
                CartItemId = item.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                PriceSnapshot = item.PriceSnapshot,
                CustomizationNotes = item.CustomizationNotes,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }

    private static OrderDetailDto MapOrder(Order order)
    {
        var subtotal = order.CalculateSubtotal();
        var total = order.CalculateTotal(0, subtotal.Currency);

        return new OrderDetailDto
        {
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Subtotal = subtotal.Amount,
            Total = total.Amount,
            Currency = subtotal.Currency,
            DeliveryAddress = order.DeliveryAddress is null
                ? null
                : new AddressDto
                {
                    Street = order.DeliveryAddress.Street,
                    City = order.DeliveryAddress.City,
                    Pincode = order.DeliveryAddress.Pincode,
                    Latitude = order.DeliveryAddress.Latitude,
                    Longitude = order.DeliveryAddress.Longitude,
                    AddressType = order.DeliveryAddress.AddressType
                },
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                OrderItemId = item.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPriceSnapshot,
                CustomizationNotes = item.CustomizationNotes,
                Subtotal = item.Subtotal
            }).ToList(),
            Payment = order.Payment is null
                ? null
                : new PaymentDto
                {
                    PaymentId = order.Payment.Id,
                    PaymentMethod = order.Payment.PaymentMethod,
                    PaymentStatus = order.Payment.PaymentStatus,
                    Amount = order.Payment.Amount.Amount,
                    Currency = order.Payment.Amount.Currency,
                    RefundedAmount = order.Payment.RefundedAmount?.Amount,
                    TransactionId = order.Payment.TransactionId,
                    FailureReason = order.Payment.FailureReason,
                    ProcessedAt = order.Payment.ProcessedAt
                },
            DeliveryAssignment = order.DeliveryAssignment is null
                ? null
                : new DeliveryAssignmentDto
                {
                    DeliveryAssignmentId = order.DeliveryAssignment.Id,
                    DeliveryAgentId = order.DeliveryAssignment.DeliveryAgentId,
                    AssignedAt = order.DeliveryAssignment.AssignedAt,
                    PickedUpAt = order.DeliveryAssignment.PickedUpAt,
                    DeliveredAt = order.DeliveryAssignment.DeliveredAt,
                    CurrentStatus = order.DeliveryAssignment.CurrentStatus
                }
        };
    }
}