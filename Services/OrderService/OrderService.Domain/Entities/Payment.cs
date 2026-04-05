namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Money Amount { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

    public string? TransactionId { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public Money? RefundedAmount { get; private set; }

    private Payment()
    {
        Amount = Money.Zero();
    }

    public Payment(Guid orderId, Money amount, PaymentMethod paymentMethod)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
    }

    public void MarkAsSuccess(string transactionId, DateTime atUtc)
    {
        if (PaymentStatus != PaymentStatus.Pending)
        {
            throw new PaymentAlreadyProcessedException(Id);
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));
        }

        TransactionId = transactionId.Trim();
        PaymentStatus = PaymentStatus.Success;
        ProcessedAt = atUtc;
        Touch();
    }

    public void MarkAsFailed(string reason, DateTime atUtc)
    {
        if (PaymentStatus != PaymentStatus.Pending)
        {
            throw new PaymentAlreadyProcessedException(Id);
        }

        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Unknown failure" : reason.Trim();
        PaymentStatus = PaymentStatus.Failed;
        ProcessedAt = atUtc;
        Touch();
    }

    public void InitiateRefund(Money refundAmount, DateTime atUtc)
    {
        if (PaymentStatus != PaymentStatus.Success)
        {
            throw new PaymentException("Refund can only be initiated for successful payments.");
        }

        if (!refundAmount.Currency.Equals(Amount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentException("Refund currency must match payment currency.");
        }

        if (refundAmount.Amount <= 0 || refundAmount.Amount > Amount.Amount)
        {
            throw new InvalidRefundAmountException(refundAmount.Amount, Amount.Amount);
        }

        RefundedAmount = refundAmount;
        PaymentStatus = PaymentStatus.RefundInitiated;
        ProcessedAt = atUtc;
        Touch();
    }

    public void CompleteRefund(DateTime atUtc)
    {
        if (PaymentStatus != PaymentStatus.RefundInitiated)
        {
            throw new PaymentException("Refund must be initiated before completion.");
        }

        PaymentStatus = PaymentStatus.Refunded;
        ProcessedAt = atUtc;
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}