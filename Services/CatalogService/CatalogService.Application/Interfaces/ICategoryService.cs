namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.Category;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesByRestaurantAsync(Guid restaurantId);

    Task<CategoryDto> GetCategoryByIdAsync(Guid id);

    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId, string userRole);

    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto, Guid userId, string userRole);

    Task<bool> DeleteCategoryAsync(Guid id, Guid userId, string userRole);

    Task<List<CategoryDto>> ReorderCategoriesAsync(Guid restaurantId, List<Guid> categoryIds);
}
