using AutoMapper;
using CatalogService.Application.DTOs.Category;
using CatalogService.Application.DTOs.OperatingHours;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Mappers;

public class RestaurantMappingProfile : Profile
{
    public RestaurantMappingProfile()
    {
        // Restaurant -> RestaurantDto
        CreateMap<Restaurant, RestaurantDto>()
            .ReverseMap();

        // Restaurant -> RestaurantDetailDto
        CreateMap<Restaurant, RestaurantDetailDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories))
            .ForMember(dest => dest.MenuItems, opt => opt.MapFrom(src => src.MenuItems))
            .ForMember(dest => dest.OperatingHours, opt => opt.MapFrom(src => src.OperatingHours))
            .ReverseMap();

        // CreateRestaurantDto -> Restaurant
        CreateMap<CreateRestaurantDto, Restaurant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.OperatingHours, opt => opt.Ignore());

        // UpdateRestaurantDto -> Restaurant
        CreateMap<UpdateRestaurantDto, Restaurant>()
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.OperatingHours, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
