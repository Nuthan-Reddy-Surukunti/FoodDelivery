namespace OrderService.Application.DTOs.Requests;

public class PlaceOrderRequestDto
{
    public Guid UserId { get; set; }
    
    public Guid RestaurantId { get; set; }
    
    public Guid SelectedAddressId { get; set; }
    
    public string? SpecialInstructions { get; set; }
}
