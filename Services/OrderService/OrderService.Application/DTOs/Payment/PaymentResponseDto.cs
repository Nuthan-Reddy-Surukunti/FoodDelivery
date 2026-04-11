namespace OrderService.Application.DTOs.Payment;

using OrderService.Application.DTOs.Delivery;
using OrderService.Domain.Enums;

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public PaymentMethod PaymentMethod { get; set; }

    public DateTime ProcessedAt { get; set; }

    public DeliveryAssignmentDto? DeliveryAssignment { get; set; }

    public string? ErrorMessage { get; set; }
}
