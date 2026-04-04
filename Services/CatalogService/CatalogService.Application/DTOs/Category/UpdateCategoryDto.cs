namespace CatalogService.Application.DTOs.Category;

using System.ComponentModel.DataAnnotations;

public class UpdateCategoryDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(150)]
    public string? Name { get; set; }

    public int? DisplayOrder { get; set; }
}
