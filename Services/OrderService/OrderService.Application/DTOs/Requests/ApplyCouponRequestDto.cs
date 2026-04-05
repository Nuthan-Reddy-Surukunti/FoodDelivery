namespace OrderService.Application.DTOs.Requests;

public class ApplyCouponRequestDto
{
    public Guid UserId { get; set; }
    
    public Guid RestaurantId { get; set; }
    
    public string CouponCode { get; set; } = string.Empty;
}
