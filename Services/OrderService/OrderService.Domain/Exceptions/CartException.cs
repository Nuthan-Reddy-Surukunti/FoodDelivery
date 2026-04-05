namespace OrderService.Domain.Exceptions;

public class CartException : Exception
{
    public CartException(string message)
        : base(message)
    {
    }
}

public sealed class MixedCartException : CartException
{
    public MixedCartException(Guid cartRestaurantId, Guid itemRestaurantId)
        : base($"Cannot add item from restaurant {itemRestaurantId} to cart bound to restaurant {cartRestaurantId}.")
    {
    }
}

public sealed class CartEmptyException : CartException
{
    public CartEmptyException(Guid cartId)
        : base($"Cart '{cartId}' is empty.")
    {
    }
}

public sealed class CartNotFoundException : CartException
{
    public CartNotFoundException(Guid cartId)
        : base($"Cart '{cartId}' was not found.")
    {
    }
}