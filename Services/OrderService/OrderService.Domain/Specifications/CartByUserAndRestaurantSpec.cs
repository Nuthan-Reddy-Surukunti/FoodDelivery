namespace OrderService.Domain.Specifications;

using OrderService.Domain.Entities;

public sealed class CartByUserAndRestaurantSpec
{
    public Guid UserId { get; }

    public Guid RestaurantId { get; }

    public CartByUserAndRestaurantSpec(Guid userId, Guid restaurantId)
    {
        UserId = userId;
        RestaurantId = restaurantId;
    }

    public bool IsSatisfiedBy(Cart cart)
    {
        return cart.UserId == UserId && cart.RestaurantId == RestaurantId;
    }
}