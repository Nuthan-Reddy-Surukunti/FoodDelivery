namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class ProcessPaymentRequestDto
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }

    // Card payment details (required when PaymentMethod == Card)
    public string? CardNumber { get; set; }       // Full number sent by frontend; backend masks it
    public string? CardExpiry { get; set; }
    public string? CardCvv { get; set; }
    public string? CardHolderName { get; set; }

    // Wallet payment details (required when PaymentMethod == Wallet)
    public string? WalletId { get; set; }
}
