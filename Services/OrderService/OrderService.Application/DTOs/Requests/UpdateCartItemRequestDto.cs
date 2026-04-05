namespace OrderService.Application.DTOs.Requests;

public class UpdateCartItemRequestDto
{
    public Guid UserId { get; set; }
    
    public Guid CartItemId { get; set; }
    
    public int NewQuantity { get; set; }
}
