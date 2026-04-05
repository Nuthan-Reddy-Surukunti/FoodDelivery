namespace OrderService.Application.DTOs.Payment;

using OrderService.Domain.Enums;

public class PaymentDto
{
    public Guid PaymentId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public decimal? RefundedAmount { get; set; }

    public string? TransactionId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? ProcessedAt { get; set; }
}