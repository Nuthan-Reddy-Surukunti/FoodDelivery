namespace OrderService.Application.Interfaces;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Checkout;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Requests;

public interface IOrderWorkflowService
{
    Task<CartDto> GetOrCreateCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<CartDto> AddCartItemAsync(AddCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> RemoveCartItemAsync(RemoveCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> ClearCartAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default);

    Task<CartDto> UpdateCartItemAsync(UpdateCartItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CartDto> ApplyCouponAsync(ApplyCouponRequestDto request, CancellationToken cancellationToken = default);

    Task<CheckoutContextDto> GetCheckoutContextAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ValidateCheckoutAsync(CheckoutValidationRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> PlaceOrderAsync(PlaceOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> CheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> SimulatePaymentAsync(SimulatePaymentRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> AssignDeliveryAsync(AssignDeliveryRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> UpdateOrderStatusAsync(UpdateOrderStatusRequestDto request, CancellationToken cancellationToken = default);

    Task<OrderDetailDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDetailDto>> GetOrdersByUserAsync(Guid userId, bool activeOnly = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderTimelineEntryDto>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default);
}
