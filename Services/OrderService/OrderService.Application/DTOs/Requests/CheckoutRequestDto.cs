namespace OrderService.Application.DTOs.Requests;

using OrderService.Application.DTOs.Common;

public class CheckoutRequestDto
{
    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public AddressDto DeliveryAddress { get; set; } = new();
}