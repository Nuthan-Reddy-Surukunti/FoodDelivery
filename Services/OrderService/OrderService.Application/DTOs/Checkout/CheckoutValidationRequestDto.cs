namespace OrderService.Application.DTOs.Checkout;

public class CheckoutValidationRequestDto
{
    public Guid UserId { get; set; }
    
    public Guid RestaurantId { get; set; }
    
    public Guid SelectedAddressId { get; set; }
    
    public Guid SelectedTimeSlotId { get; set; }
    
    public string? SpecialInstructions { get; set; }
}
