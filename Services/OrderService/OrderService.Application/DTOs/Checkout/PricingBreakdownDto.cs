namespace OrderService.Application.DTOs.Checkout;

public class PricingBreakdownDto
{
    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Discount { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = "INR";

    public decimal TaxPercentage { get; set; }
}
