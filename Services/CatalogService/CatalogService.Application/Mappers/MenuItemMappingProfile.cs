using AutoMapper;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Mappers;

public class MenuItemMappingProfile : Profile
{
    public MenuItemMappingProfile()
    {
        // MenuItem -> MenuItemDto (with CategoryName mapping)
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name))
            .ReverseMap();

        // CreateMenuItemDto -> MenuItem
        CreateMap<CreateMenuItemDto, MenuItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Restaurant, opt => opt.Ignore());

        // UpdateMenuItemDto -> MenuItem
        CreateMap<UpdateMenuItemDto, MenuItem>()
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Restaurant, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
