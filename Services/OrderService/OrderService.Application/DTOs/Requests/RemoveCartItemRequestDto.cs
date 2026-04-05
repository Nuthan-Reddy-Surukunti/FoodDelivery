namespace OrderService.Application.DTOs.Requests;

public class RemoveCartItemRequestDto
{
    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public Guid CartItemId { get; set; }
}