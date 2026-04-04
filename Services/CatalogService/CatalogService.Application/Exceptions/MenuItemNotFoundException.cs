namespace CatalogService.Application.Exceptions;

public class MenuItemNotFoundException : Exception
{
    public MenuItemNotFoundException(Guid id)
        : base($"Menu item with ID '{id}' not found.")
    {
    }
}
