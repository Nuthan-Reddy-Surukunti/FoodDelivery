using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;

namespace AdminService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        // Restaurant mappings
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Address.State))
            .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Address.ZipCode))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Country))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactInfo.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.ContactInfo.Phone))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.TotalAmount.Currency));

        // Report mappings
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.TotalOrders, opt => opt.MapFrom(src => src.Metrics.TotalOrders))
            .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Metrics.TotalRevenue.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Metrics.TotalRevenue.Currency))
            .ForMember(dest => dest.TotalCustomers, opt => opt.MapFrom(src => src.Metrics.TotalCustomers))
            .ForMember(dest => dest.TotalRestaurants, opt => opt.MapFrom(src => src.Metrics.TotalRestaurants))
            .ForMember(dest => dest.AverageOrderValue, opt => opt.MapFrom(src => src.Metrics.AverageOrderValue));
    }
}
