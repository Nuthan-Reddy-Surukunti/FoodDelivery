using AutoMapper;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public DashboardService(
        IOrderRepository orderRepository, 
        IRestaurantRepository restaurantRepository, 
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<DashboardKpisDto> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Get total orders count
        var totalOrders = await _orderRepository.GetTotalOrdersCountAsync(cancellationToken);

        // Calculate total revenue (GMV)
        var totalRevenue = await _orderRepository.GetTotalRevenueAsync(cancellationToken);

        // Get active partners count
        var activePartners = await _restaurantRepository.GetCountByStatusAsync(RestaurantStatus.Approved, cancellationToken);

        // Get pending approvals count
        var pendingApprovals = await _restaurantRepository.GetCountByStatusAsync(RestaurantStatus.Pending, cancellationToken);

        // Get today's orders count
        var ordersToday = await _orderRepository.GetOrdersCountByDateRangeAsync(today, tomorrow, cancellationToken);

        // Calculate today's revenue
        var revenueToday = await _orderRepository.GetRevenueBetweenDatesAsync(today, tomorrow, cancellationToken);

        return new DashboardKpisDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue.Amount,
            TotalRevenueCurrency = totalRevenue.Currency,
            ActivePartners = activePartners,
            PendingApprovals = pendingApprovals,
            OrdersToday = ordersToday,
            RevenueToday = revenueToday.Amount,
            RevenueTodayCurrency = revenueToday.Currency
        };
    }

    public async Task<List<RestaurantDto>> GetApprovalQueueAsync(CancellationToken cancellationToken = default)
    {
        var pendingRestaurants = await _restaurantRepository.GetPendingApprovalsAsync(cancellationToken);
        return _mapper.Map<List<RestaurantDto>>(pendingRestaurants);
    }
}