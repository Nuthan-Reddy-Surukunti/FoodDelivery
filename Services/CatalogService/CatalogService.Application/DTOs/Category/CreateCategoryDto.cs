namespace CatalogService.Application.DTOs.Category;

using System.ComponentModel.DataAnnotations;

public class CreateCategoryDto
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;
}
