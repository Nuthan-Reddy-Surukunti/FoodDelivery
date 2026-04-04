namespace CatalogService.Application.Exceptions;

public class RestaurantNotFoundException : Exception
{
    public RestaurantNotFoundException(Guid id)
        : base($"Restaurant with ID '{id}' not found.")
    {
    }

    public RestaurantNotFoundException(string message)
        : base(message)
    {
    }
}
