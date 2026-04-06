namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;
using OrderService.Domain.Enums;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public CartStatus Status { get; set; } = CartStatus.Active;
    public ICollection<CartItem> Items { get; set; } = [];
}