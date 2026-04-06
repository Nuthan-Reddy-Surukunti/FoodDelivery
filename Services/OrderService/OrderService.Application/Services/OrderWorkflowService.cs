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
using OrderService.Domain.Constants;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Interfaces;

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
            cart = new Cart { UserId = userId, RestaurantId = restaurantId };
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        return MapCart(cart);
    }

    public async Task<CartDto> AddCartItemAsync(AddCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ValidateIdentity(request.MenuItemId, nameof(request.MenuItemId));

        if (request.Quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0.");
        }

        var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(request.UserId, request.RestaurantId, cancellationToken);
        var isNewCart = cart is null;
        cart ??= new Cart { UserId = request.UserId, RestaurantId = request.RestaurantId };

        if (cart.Status != CartStatus.Active)
        {
            throw new CartException("Only active carts can be modified.");
        }

        var normalizedNotes = string.IsNullOrWhiteSpace(request.CustomizationNotes) ? null : request.CustomizationNotes.Trim();
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.MenuItemId == request.MenuItemId &&
            string.Equals(i.CustomizationNotes, normalizedNotes, StringComparison.Ordinal));

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                MenuItemId = request.MenuItemId,
                Quantity = request.Quantity,
                PriceSnapshot = request.PriceSnapshot,
                CustomizationNotes = normalizedNotes
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

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

        var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (itemToRemove is not null)
        {
            cart.Items.Remove(itemToRemove);
            cart.UpdatedAt = DateTime.UtcNow;
        }

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

        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
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

        existingCartItem.Quantity = request.NewQuantity;
        existingCartItem.UpdatedAt = DateTime.UtcNow;
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

        // Coupon tracking is not persisted; validation would call an external CouponService
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
                City = "Bengaluru",
                Pincode = "560001",
                AddressType = AddressType.Home,
                Latitude = 37.7749,
                Longitude = -122.4194
            },
            new()
            {
                Street = "456 Work Ave",
                City = "Bengaluru",
                Pincode = "560002",
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

        return true;
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateIdentity(request.UserId, nameof(request.UserId));
        ValidateIdentity(request.RestaurantId, nameof(request.RestaurantId));
        ValidateIdentity(request.SelectedAddressId, nameof(request.SelectedAddressId));
        ValidateIdentity(request.SelectedTimeSlotId, nameof(request.SelectedTimeSlotId));

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

        var order = new Order { UserId = request.UserId, RestaurantId = request.RestaurantId };
        foreach (var cartItem in cart.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = cartItem.MenuItemId,
                Quantity = cartItem.Quantity,
                UnitPriceSnapshot = cartItem.PriceSnapshot,
                CustomizationNotes = cartItem.CustomizationNotes
            });
        }

        // In a real app, get actual address details from UserService/AddressService
        var now = DateTime.UtcNow;
        order.DeliveryStreet = "123 Main St";
        order.DeliveryCity = "Bengaluru";
        order.DeliveryPincode = "560001";
        order.DeliveryAddressType = AddressType.Home;
        order.DeliveryLatitude = 37.7749;
        order.DeliveryLongitude = -122.4194;

        TransitionOrderStatus(order, OrderStatus.CheckoutStarted, now);
        TransitionOrderStatus(order, OrderStatus.PaymentPending, now);

        await _orderRepository.AddAsync(order, cancellationToken);

        // Clear cart after order creation
        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
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

        var order = new Order { UserId = request.UserId, RestaurantId = request.RestaurantId };
        foreach (var cartItem in cart.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = cartItem.MenuItemId,
                Quantity = cartItem.Quantity,
                UnitPriceSnapshot = cartItem.PriceSnapshot,
                CustomizationNotes = cartItem.CustomizationNotes
            });
        }

        var now = DateTime.UtcNow;
        order.DeliveryStreet = request.DeliveryAddress.Street;
        order.DeliveryCity = request.DeliveryAddress.City;
        order.DeliveryPincode = request.DeliveryAddress.Pincode;
        order.DeliveryAddressType = request.DeliveryAddress.AddressType;
        order.DeliveryLatitude = request.DeliveryAddress.Latitude;
        order.DeliveryLongitude = request.DeliveryAddress.Longitude;

        TransitionOrderStatus(order, OrderStatus.CheckoutStarted, now);
        TransitionOrderStatus(order, OrderStatus.PaymentPending, now);

        await _orderRepository.AddAsync(order, cancellationToken);

        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return MapOrder(order);
    }

    public async Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderByIdWithItemsAsync(request.OrderId, cancellationToken)
                    ?? await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", request.OrderId);

        var amountValue = request.Amount.HasValue && request.Amount.Value > 0
            ? request.Amount.Value
            : CalculateOrderTotal(order, request.TaxPercentage);

        var isNewPayment = order.Payment is null;
        var payment = order.Payment ?? new Payment
        {
            OrderId = order.Id,
            Amount = amountValue,
            Currency = "USD",
            PaymentMethod = request.PaymentMethod
        };

        var now = DateTime.UtcNow;
        if (request.IsSuccessful)
        {
            var transactionId = string.IsNullOrWhiteSpace(request.TransactionId)
                ? $"SIM-{Guid.NewGuid():N}"[..12]
                : request.TransactionId.Trim();

            payment.TransactionId = transactionId;
            payment.PaymentStatus = PaymentStatus.Success;
            payment.ProcessedAt = now;
            payment.UpdatedAt = now;

            order.Payment = payment;
            TransitionOrderStatus(order, OrderStatus.Paid, now);
            order.PaymentCompletedAt = now;

            if (request.AutoAcceptByRestaurant)
            {
                var acceptTime = now.AddMinutes(1);
                TransitionOrderStatus(order, OrderStatus.RestaurantAccepted, acceptTime);
            }
        }
        else
        {
            payment.FailureReason = request.FailureReason ?? "Simulated payment failure.";
            payment.PaymentStatus = PaymentStatus.Failed;
            payment.ProcessedAt = now;
            payment.UpdatedAt = now;

            order.Payment = payment;
            TransitionOrderStatus(order, OrderStatus.PaymentFailed, now);
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

        var isEligible = order.DeliveryAssignment is null &&
                         OrderStatusTransitionPolicy.IsEligibleForDeliveryAssignment(order.OrderStatus);
        if (!isEligible)
        {
            throw new InvalidOperationException("Order is not eligible for delivery assignment.");
        }

        var assignment = new DeliveryAssignment
        {
            OrderId = order.Id,
            DeliveryAgentId = request.DeliveryAgentId,
            AssignedAt = DateTime.UtcNow
        };

        order.DeliveryAssignment = assignment;
        order.UpdatedAt = DateTime.UtcNow;

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
                TransitionOrderStatus(order, OrderStatus.RestaurantAccepted, now);
                break;
            case OrderStatus.RestaurantRejected:
                TransitionOrderStatus(order, OrderStatus.RestaurantRejected, now);
                break;
            case OrderStatus.Preparing:
                TransitionOrderStatus(order, OrderStatus.Preparing, now);
                order.PreparationStartTime = now;
                break;
            case OrderStatus.ReadyForPickup:
                TransitionOrderStatus(order, OrderStatus.ReadyForPickup, now);
                break;
            case OrderStatus.PickedUp:
                TransitionOrderStatus(order, OrderStatus.PickedUp, now);
                order.PickupTime = now;
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.PickedUpAt = now;
                    order.DeliveryAssignment.CurrentStatus = DeliveryStatus.PickedUp;
                    order.DeliveryAssignment.UpdatedAt = now;
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.OutForDelivery:
                TransitionOrderStatus(order, OrderStatus.OutForDelivery, now);
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.CurrentStatus = DeliveryStatus.InTransit;
                    order.DeliveryAssignment.UpdatedAt = now;
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.Delivered:
                TransitionOrderStatus(order, OrderStatus.Delivered, now);
                order.DeliveryTime = now;
                if (order.DeliveryAssignment is not null)
                {
                    order.DeliveryAssignment.DeliveredAt = now;
                    order.DeliveryAssignment.CurrentStatus = DeliveryStatus.Delivered;
                    order.DeliveryAssignment.UpdatedAt = now;
                    await _deliveryAssignmentRepository.UpdateAsync(order.DeliveryAssignment, cancellationToken);
                }
                break;
            case OrderStatus.CancelRequestedByCustomer:
                RequestOrderCancellation(order, now);
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

    public async Task<OrderDetailDto> CancelOrderAsync(Guid orderId, bool forceByAdmin = false, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
                    ?? throw new ResourceNotFoundException("Order", orderId);

        var now = DateTime.UtcNow;
        if (forceByAdmin)
        {
            order.OrderStatus = OrderStatus.CancelRequestedByCustomer;
            order.CancelRequestedAt = now;
            order.UpdatedAt = now;
        }
        else
        {
            RequestOrderCancellation(order, now);
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

    private static void TransitionOrderStatus(Order order, OrderStatus nextStatus, DateTime atUtc)
    {
        if (!OrderStatusTransitionPolicy.CanTransition(order.OrderStatus, nextStatus))
        {
            throw new InvalidOrderStatusTransitionException(order.OrderStatus, nextStatus);
        }

        order.OrderStatus = nextStatus;
        order.UpdatedAt = atUtc;
    }

    private static void RequestOrderCancellation(Order order, DateTime now)
    {
        if (order.PreparationStartTime.HasValue)
        {
            throw new OrderCancellationNotAllowedException(order.OrderStatus);
        }

        if (order.CreatedAt.AddMinutes(DomainConstants.CustomerCancellationWindowMinutes) < now)
        {
            throw new OrderCancellationNotAllowedException(order.OrderStatus);
        }

        if (!OrderStatusTransitionPolicy.CanCustomerCancel(order.OrderStatus))
        {
            throw new OrderCancellationNotAllowedException(order.OrderStatus);
        }

        TransitionOrderStatus(order, OrderStatus.CancelRequestedByCustomer, now);
        order.CancelRequestedAt = now;
    }

    private static void InitiateOrderRefund(Order order, decimal? refundAmount, DateTime now)
    {
        if (order.Payment is null)
        {
            throw new ValidationException("Payment must exist before refund initiation.");
        }

        var refundAmt = refundAmount.HasValue ? refundAmount.Value : order.Payment.Amount;

        if (refundAmt <= 0 || refundAmt > order.Payment.Amount)
        {
            throw new InvalidRefundAmountException(refundAmt, order.Payment.Amount);
        }

        order.Payment.RefundedAmount = refundAmt;
        order.Payment.RefundedCurrency = order.Payment.Currency;
        order.Payment.PaymentStatus = PaymentStatus.RefundInitiated;
        order.Payment.ProcessedAt = now;
        order.Payment.UpdatedAt = now;

        TransitionOrderStatus(order, OrderStatus.RefundInitiated, now);
    }

    private static void CompleteOrderRefund(Order order, DateTime now)
    {
        if (order.Payment is null)
        {
            throw new ValidationException("Payment must exist before refund completion.");
        }

        if (order.Payment.PaymentStatus != PaymentStatus.RefundInitiated)
        {
            InitiateOrderRefund(order, null, now);
        }

        order.Payment.PaymentStatus = PaymentStatus.Refunded;
        order.Payment.ProcessedAt = now;
        order.Payment.UpdatedAt = now;

        TransitionOrderStatus(order, OrderStatus.Refunded, now);
    }

    private static decimal CalculateOrderSubtotal(Order order)
    {
        return order.OrderItems.Sum(item => item.Subtotal);
    }

    private static decimal CalculateOrderTotal(Order order, decimal taxPercentage)
    {
        var subtotal = CalculateOrderSubtotal(order);
        var tax = decimal.Round(subtotal * taxPercentage / 100, 2, MidpointRounding.AwayFromZero);
        return subtotal + tax;
    }

    private static CartDto MapCart(Cart cart)
    {
        var total = cart.Items.Sum(item => item.Subtotal);
        return new CartDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            RestaurantId = cart.RestaurantId,
            Status = cart.Status,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            TotalAmount = total,
            Currency = "USD",
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
        var subtotal = CalculateOrderSubtotal(order);

        return new OrderDetailDto
        {
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Subtotal = subtotal,
            Total = subtotal,
            Currency = "USD",
            DeliveryAddress = order.DeliveryStreet is null
                ? null
                : new AddressDto
                {
                    Street = order.DeliveryStreet,
                    City = order.DeliveryCity ?? string.Empty,
                    Pincode = order.DeliveryPincode ?? string.Empty,
                    Latitude = order.DeliveryLatitude,
                    Longitude = order.DeliveryLongitude,
                    AddressType = order.DeliveryAddressType ?? AddressType.Home
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
                    Amount = order.Payment.Amount,
                    Currency = order.Payment.Currency,
                    RefundedAmount = order.Payment.RefundedAmount,
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
