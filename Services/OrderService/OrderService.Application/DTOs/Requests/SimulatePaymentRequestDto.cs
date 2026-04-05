namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class SimulatePaymentRequestDto
{
    public Guid OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public bool IsSuccessful { get; set; }

    public decimal? Amount { get; set; }

    public decimal TaxPercentage { get; set; }

    public string? TransactionId { get; set; }

    public string? FailureReason { get; set; }

    public bool AutoAcceptByRestaurant { get; set; } = true;
}