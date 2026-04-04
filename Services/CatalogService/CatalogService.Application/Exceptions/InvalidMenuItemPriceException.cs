namespace CatalogService.Application.Exceptions;

public class InvalidMenuItemPriceException : Exception
{
    public InvalidMenuItemPriceException(decimal price)
        : base($"Menu item price must be greater than 0. Provided: {price}")
    {
    }
}
