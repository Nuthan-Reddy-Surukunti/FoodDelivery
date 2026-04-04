namespace CatalogService.Application.Exceptions;

public class DuplicateCategoryException : Exception
{
    public DuplicateCategoryException(string categoryName, Guid restaurantId)
        : base($"Category '{categoryName}' already exists for restaurant ID '{restaurantId}'.")
    {
    }
}
