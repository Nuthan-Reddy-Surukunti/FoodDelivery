namespace OrderService.Application.DTOs.Checkout;

using OrderService.Application.DTOs.Cart;
using OrderService.Application.DTOs.Common;

public class CheckoutContextDto
{
    public CartDto Cart { get; set; } = new();
    
    public List<AddressDto> SavedAddresses { get; set; } = new();
    
    public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    
    public decimal EstimatedDeliveryCharge { get; set; }
    
    public int EstimatedDeliveryMinutes { get; set; }
}
