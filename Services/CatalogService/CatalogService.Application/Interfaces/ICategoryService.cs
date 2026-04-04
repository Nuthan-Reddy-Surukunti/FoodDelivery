namespace CatalogService.Application.Interfaces;

using CatalogService.Application.DTOs.Category;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesByRestaurantAsync(Guid restaurantId);

    Task<CategoryDto> GetCategoryByIdAsync(Guid id);

    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);

    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto);

    Task<bool> DeleteCategoryAsync(Guid id);

    Task<List<CategoryDto>> ReorderCategoriesAsync(Guid restaurantId, List<Guid> categoryIds);
}
