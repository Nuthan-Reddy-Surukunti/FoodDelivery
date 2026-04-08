using AutoMapper;
using AdminService.Application.DTOs;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        // Restaurant mappings
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // MenuItem mappings
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
            .ForMember(dest => dest.CanBeOrdered, opt => opt.MapFrom(src =>
                src.Status == MenuItemStatus.Active && src.ApprovalStatus == ApprovalStatus.Approved));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Report mappings
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.Type.ToString()));

        // AuditLog mappings
        CreateMap<AuditLog, AuditLogDto>();
    }
}
