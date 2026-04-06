namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class ProcessPaymentRequestDto
{
    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }
}
