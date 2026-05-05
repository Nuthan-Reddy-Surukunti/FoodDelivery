using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Enums;
using Razorpay.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MassTransit;
using QuickBite.Shared.Events.Order;
using QuickBite.Shared.Contracts;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/payments")]
[Authorize(Roles = "Customer")]
public class PaymentsController : ControllerBase
{
    private readonly IOrderPlacementService _orderPlacementService;
    private readonly IDeliveryService _deliveryService;
    private readonly IConfiguration _configuration;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IOrderPlacementService orderPlacementService,
        IDeliveryService deliveryService,
        IConfiguration configuration,
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<PaymentsController> logger)
    {
        _orderPlacementService = orderPlacementService;
        _deliveryService = deliveryService;
        _configuration = configuration;
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("{orderId:guid}/process")]
    public async Task<IActionResult> ProcessPayment(
        [FromRoute] Guid orderId,
        [FromBody] ProcessPaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (orderId == Guid.Empty)
        {
            return BadRequest("OrderId is required");
        }

        var order = await _orderPlacementService.GetOrderByIdAsync(orderId, cancellationToken);
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty || order.UserId != currentUserId)
        {
            return Forbid();
        }

        var paymentResponse = await _deliveryService.ProcessPaymentAsync(orderId, request, cancellationToken);
        return Ok(paymentResponse);
    }

    [HttpPost("{orderId:guid}/create-razorpay-order")]
    public async Task<IActionResult> CreateRazorpayOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderPlacementService.GetOrderByIdAsync(orderId, cancellationToken);
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty || order.UserId != currentUserId)
        {
            return Forbid();
        }

        string keyId = _configuration["Razorpay:KeyId"] ?? "";
        string keySecret = _configuration["Razorpay:KeySecret"] ?? "";

        RazorpayClient client = new RazorpayClient(keyId, keySecret);
        
        Dictionary<string, object> options = new Dictionary<string, object>();
        options.Add("amount", (int)(order.Total * 100)); 
        options.Add("currency", "INR");
        options.Add("receipt", orderId.ToString());

        Razorpay.Api.Order razorpayOrder = client.Order.Create(options);

        return Ok(new { RazorpayOrderId = razorpayOrder["id"].ToString() });
    }

    [HttpPost("{orderId:guid}/verify")]
    public async Task<IActionResult> VerifyRazorpayPayment(
        [FromRoute] Guid orderId,
        [FromBody] VerifyRazorpayPaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        var orderDto = await _orderPlacementService.GetOrderByIdAsync(orderId, cancellationToken);
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty || orderDto.UserId != currentUserId)
        {
            return Forbid();
        }

        string keySecret = _configuration["Razorpay:KeySecret"] ?? "";
        
        try
        {
            // Manual verification to be safe, or use the Razorpay Utils with the secret
            string payload = request.RazorpayOrderId + "|" + request.RazorpayPaymentId;
            
            // This is the standard Razorpay signature verification logic
            var secretBytes = System.Text.Encoding.UTF8.GetBytes(keySecret);
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            
            using (var hmac = new System.Security.Cryptography.HMACSHA256(secretBytes))
            {
                var hashBytes = hmac.ComputeHash(payloadBytes);
                string generatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                
                if (generatedSignature != request.RazorpaySignature.ToLower())
                {
                    return BadRequest(new { Message = "Payment verification failed: Invalid Signature" });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment verification failed for order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "Payment verification failed.",
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "PAYMENT_VERIFICATION_FAILED"
            });
        }

        // Signature is valid. Update order status to Paid.
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order != null)
        {
            order.OrderStatus = OrderStatus.Paid;
            order.PaymentCompletedAt = DateTime.UtcNow;
            
            // Add or update Payment record
            if (order.Payment == null) 
            {
                order.Payment = new OrderService.Domain.Entities.Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    PaymentMethod = PaymentMethod.Online, 
                    PaymentStatus = PaymentStatus.Success,
                    ProcessedAt = DateTime.UtcNow,
                    RazorpayOrderId = request.RazorpayOrderId,
                    RazorpayPaymentId = request.RazorpayPaymentId,
                    RazorpaySignature = request.RazorpaySignature
                };
            }
            else
            {
                order.Payment.PaymentMethod = PaymentMethod.Online;
                order.Payment.PaymentStatus = PaymentStatus.Success;
                order.Payment.ProcessedAt = DateTime.UtcNow;
                order.Payment.RazorpayOrderId = request.RazorpayOrderId;
                order.Payment.RazorpayPaymentId = request.RazorpayPaymentId;
                order.Payment.RazorpaySignature = request.RazorpaySignature;
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);

            // ASSIGN DELIVERY AGENT for this online order
            await _deliveryService.AssignDeliveryAgentAsync(orderId, cancellationToken);

            // Clear the cart
            var cart = await _cartRepository.GetCartByUserAndRestaurantAsync(order.UserId, order.RestaurantId, cancellationToken);
            if (cart != null)
            {
                cart.Status = OrderService.Domain.Enums.CartStatus.Abandoned;
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateAsync(cart, cancellationToken);
            }

            // Publish OrderPlacedEvent since we deferred it for online payments
            await _publishEndpoint.Publish(new OrderPlacedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                EventVersion = 1,
                OrderId = order.Id,
                UserId = order.UserId,
                RestaurantId = order.RestaurantId,
                RestaurantName = "Restaurant",
                TotalAmount = order.TotalAmount,
                DeliveryAddress = $"{order.DeliveryAddressLine1}, {order.DeliveryCity}",
                PaymentMethod = "Online",           // Razorpay path — Saga uses this to trigger refund on failure
                PaymentId = order.Payment?.Id,     // Internal Payment ID for the Saga to look up RazorpayPaymentId
                Items = order.OrderItems.Select(oi => new OrderItemSnapshot
                {
                    MenuItemId = oi.MenuItemId,
                    MenuItemName = "Menu Item",
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.UnitPrice
                }).ToList()
            }, cancellationToken);

            // Publish OrderStatusChangedEvent for tracking consistency
            await _publishEndpoint.Publish(new OrderStatusChangedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                EventVersion = 1,
                OrderId = order.Id,
                UserId = order.UserId,
                RestaurantId = order.RestaurantId,
                OldStatus = OrderStatus.CheckoutStarted.ToString(),
                NewStatus = OrderStatus.Paid.ToString()
            }, cancellationToken);
        }

        return Ok(new { Success = true, Message = "Payment verified successfully" });
    }
}
