using AutoMapper;
using AdminService.Application.DTOs;
using AdminService.Application.DTOs.Responses;
using AdminService.Domain.Entities;

namespace AdminService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        // Restaurant mappings
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Report mappings
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.Type.ToString()));

        // AuditLog mappings
        CreateMap<AuditLog, AuditLogDto>();

        // User mappings
        CreateMap<User, UserDto>();
    }
}
