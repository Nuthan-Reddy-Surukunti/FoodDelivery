namespace CatalogService.Application.Exceptions;

public class InvalidRestaurantDataException : Exception
{
    public InvalidRestaurantDataException(string validationErrors)
        : base($"Restaurant data validation failed: {validationErrors}")
    {
    }

    public InvalidRestaurantDataException(List<string> errors)
        : base($"Restaurant data validation failed: {string.Join(", ", errors)}")
    {
    }
}
