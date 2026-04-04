namespace CatalogService.Application.Exceptions;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(Guid id)
        : base($"Category with ID '{id}' not found.")
    {
    }
}
