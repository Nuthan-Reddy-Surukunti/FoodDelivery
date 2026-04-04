using AutoMapper;
using CatalogService.Application.DTOs.Category;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Mappers;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        // Category -> CategoryDto (with ItemCount calculation)
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.MenuItems.Count))
            .ReverseMap();

        // CreateCategoryDto -> Category
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore())
            .ForMember(dest => dest.Restaurant, opt => opt.Ignore());

        // UpdateCategoryDto -> Category
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore())
            .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
