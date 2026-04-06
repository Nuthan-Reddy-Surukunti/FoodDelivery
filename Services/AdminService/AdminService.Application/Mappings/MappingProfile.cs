using AutoMapper;
using AdminService.Application.DTOs;
using AdminService.Application.DTOs.Responses;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;

namespace AdminService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Restaurant: ContactEmail/ContactPhone -> Email/Phone in DTO
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.ContactPhone))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // MenuItem: flat Price/Currency
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
            .ForMember(dest => dest.CanBeOrdered, opt => opt.MapFrom(src =>
                src.Status == MenuItemStatus.Active && src.ApprovalStatus == ApprovalStatus.Approved));

        // Order: flat TotalAmount/Currency
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Report: flat metrics, Type -> ReportType string
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.Type.ToString()));

        // AuditLog: all properties match directly
        CreateMap<AuditLog, AuditLogDto>();
    }
}
