using AutoMapper;
using CatalogService.Application.DTOs.Category;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repository, IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _repository = repository;
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetCategoriesByRestaurantAsync(Guid restaurantId, string? userRole = null, Guid? userId = null)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(restaurantId);

        // Allow RestaurantPartner to manage categories for their own restaurant (any status)
        // Allow Admin to manage all. Other users only see categories for Active restaurants
        if (userRole == "RestaurantPartner" && userId.HasValue && restaurant.OwnerId == userId)
        {
            // RestaurantPartner can access their own restaurant even if Pending
        }
        else if (userRole != "Admin" && restaurant.Status != CatalogService.Domain.Enums.RestaurantStatus.Active)
        {
            // Non-owners and non-admins cannot access non-active restaurants
            throw new RestaurantNotFoundException(restaurantId);
        }

        var categories = await _repository.GetByRestaurantAsync(restaurantId);
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id, string? userRole = null)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            throw new CategoryNotFoundException(id);

        // Check if parent restaurant is active unless user is Admin
        if (userRole != "Admin")
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(category.RestaurantId);
            if (restaurant == null || restaurant.Status != CatalogService.Domain.Enums.RestaurantStatus.Active)
                throw new CategoryNotFoundException(id);
        }

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId, string userRole)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(dto.RestaurantId);
        
        // RestaurantPartner can only create categories for their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only create categories for your own restaurant.");

        // Check for duplicate category name
        var existingCategory = await _repository.GetByNameAsync(dto.Name, dto.RestaurantId);
        if (existingCategory != null)
            throw new DuplicateCategoryException(dto.Name, dto.RestaurantId);

        var category = _mapper.Map<Category>(dto);
        var createdCategory = await _repository.CreateAsync(category);
        
        return _mapper.Map<CategoryDto>(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto, Guid userId, string userRole)
    {
        if (dto.Id == Guid.Empty)
            throw new InvalidRestaurantDataException("Category ID is required.");

        var category = await _repository.GetByIdAsync(dto.Id);
        if (category == null)
            throw new CategoryNotFoundException(dto.Id);
        
        // Get parent restaurant to validate ownership
        var restaurant = await _restaurantRepository.GetByIdAsync(category.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(category.RestaurantId);
        
        // RestaurantPartner can only update categories in their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only update categories in your own restaurant.");

        // Check for duplicate if name is being updated
        if (!string.IsNullOrEmpty(dto.Name) && category.Name != dto.Name)
        {
            var existingCategory = await _repository.GetByNameAsync(dto.Name, category.RestaurantId);
            if (existingCategory != null && existingCategory.Id != dto.Id)
                throw new DuplicateCategoryException(dto.Name, category.RestaurantId);
        }

        _mapper.Map(dto, category);
        var updatedCategory = await _repository.UpdateAsync(category);
        
        return _mapper.Map<CategoryDto>(updatedCategory);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, Guid userId, string userRole)
    {
        var category = await _repository.GetByIdAsync(id);
        if (category == null)
            throw new CategoryNotFoundException(id);
        
        // Get parent restaurant to validate ownership
        var restaurant = await _restaurantRepository.GetByIdAsync(category.RestaurantId);
        if (restaurant == null)
            throw new RestaurantNotFoundException(category.RestaurantId);
        
        // RestaurantPartner can only delete categories from their own restaurant
        if (userRole == "RestaurantPartner" && restaurant.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only delete categories from your own restaurant.");

        // Check if category has menu items
        if (category.MenuItems.Count > 0)
            throw new InvalidRestaurantDataException($"Cannot delete category with {category.MenuItems.Count} menu item(s). Please move or remove items first.");

        return await _repository.DeleteAsync(id);
    }
}
