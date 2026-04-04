namespace CatalogService.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int ItemCount { get; set; }
}
