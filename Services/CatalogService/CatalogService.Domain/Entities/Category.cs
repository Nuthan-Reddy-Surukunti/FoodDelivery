namespace CatalogService.Domain.Entities;

using CatalogService.Domain.Common;

public class Category : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public Restaurant? Restaurant { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = [];
}
