using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;
using QuickBite.Shared.Events.Saga;
using Razorpay.Api;

namespace OrderService.Application.Consumers;

/// <summary>
/// Compensation consumer: cancels the order and issues a refund (for online payments).
/// Triggered by the Saga when validation fails after payment is confirmed.
/// </summary>
public class CompensateOrderConsumer : IConsumer<CompensateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CompensateOrderConsumer> _logger;

    public CompensateOrderConsumer(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        ILogger<CompensateOrderConsumer> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CompensateOrderCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Compensating order {OrderId} — Reason: {Reason} | ShouldRefund: {ShouldRefund}",
            message.OrderId, message.Reason, message.ShouldRefund);

        // ── Step 1: Update order status to RestaurantRejected ───────────────
        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);
        if (order is null)
        {
            _logger.LogError("Compensation failed — Order {OrderId} not found", message.OrderId);
            return;
        }

        order.OrderStatus = OrderStatus.RestaurantRejected;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepository.UpdateAsync(order, context.CancellationToken);

        _logger.LogInformation("Order {OrderId} status set to RestaurantRejected", message.OrderId);

        // ── Step 2: Refund if online payment ────────────────────────────────
        if (message.ShouldRefund && message.PaymentId.HasValue)
        {
            await TryRefundAsync(message.PaymentId.Value, message.Amount, message.OrderId, context.CancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Order {OrderId} is COD — no refund needed", message.OrderId);
        }
    }

    private async Task TryRefundAsync(Guid paymentId, decimal amount, Guid orderId, CancellationToken ct)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, ct);
            if (payment is null)
            {
                _logger.LogWarning("Refund skipped — Payment record {PaymentId} not found", paymentId);
                return;
            }

            if (string.IsNullOrEmpty(payment.RazorpayPaymentId))
            {
                _logger.LogWarning(
                    "Refund skipped — Order {OrderId} has no RazorpayPaymentId", orderId);
                return;
            }

            string keyId = _configuration["Razorpay:KeyId"] ?? "";
            string keySecret = _configuration["Razorpay:KeySecret"] ?? "";

            var client = new RazorpayClient(keyId, keySecret);

            // Razorpay amounts are in paise (multiply by 100)
            var refundOptions = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) },
                { "speed", "normal" },
                { "notes", new Dictionary<string, string>
                    {
                        { "order_id", orderId.ToString() },
                        { "reason", "Order cancelled — restaurant/item unavailable" }
                    }
                }
            };

            var razorpayPayment = client.Payment.Fetch(payment.RazorpayPaymentId);
            razorpayPayment.Refund(refundOptions);

            // Update payment status
            payment.PaymentStatus = PaymentStatus.RefundInitiated;
            payment.RefundedAmount = amount;
            await _paymentRepository.UpdateAsync(payment, ct);

            _logger.LogInformation(
                "Razorpay refund initiated for Order {OrderId}, Amount: ₹{Amount}",
                orderId, amount);
        }
        catch (Exception ex)
        {
            // Log but don't rethrow — the order is already cancelled.
            // The refund can be manually issued if the API call fails.
            _logger.LogError(ex,
                "Razorpay refund FAILED for Order {OrderId} — manual refund may be required",
                orderId);
        }
    }
}
