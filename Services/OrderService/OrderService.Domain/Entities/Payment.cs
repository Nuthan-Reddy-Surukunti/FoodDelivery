namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? TransactionId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public decimal? RefundedAmount { get; set; }

    // Card payment details (null for non-card methods)
    public string? MaskedCardNumber { get; set; }  // e.g. "**** **** **** 4242"
    public string? CardHolderName { get; set; }

    // Wallet payment details (null for non-wallet methods)
    public string? WalletId { get; set; }

    // Razorpay details
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }

    public Order? Order { get; set; }
}