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
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public DashboardService(
        IOrderRepository orderRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<DashboardKpisDto> GetKpisAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        // Fetch data sequentially because EF Core DbContext is not thread-safe
        var totalOrders       = await _orderRepository.GetTotalOrdersCountAsync(cancellationToken);
        var totalRevenue      = await _orderRepository.GetTotalRevenueAsync(cancellationToken);
        var activePartners    = await _restaurantRepository.GetCountByStatusAsync(RestaurantStatus.Active, cancellationToken);
        var pendingApprovals  = await _restaurantRepository.GetCountByStatusAsync(RestaurantStatus.Pending, cancellationToken);
        var ordersToday       = await _orderRepository.GetOrdersCountByDateRangeAsync(today, tomorrow, cancellationToken);
        var revenueToday      = await _orderRepository.GetRevenueBetweenDatesAsync(today, tomorrow, cancellationToken);
        var ordersYesterday   = await _orderRepository.GetOrdersCountByDateRangeAsync(yesterday, today, cancellationToken);
        var revenueYesterday  = await _orderRepository.GetRevenueBetweenDatesAsync(yesterday, today, cancellationToken);
        var totalUsers        = await _userRepository.GetCountAsync(cancellationToken);

        // Build 7-day daily order count map
        var dailyOrderCounts = new Dictionary<string, int>();
        for (int i = 6; i >= 0; i--)
        {
            var dayStart = today.AddDays(-i);
            var dayEnd   = dayStart.AddDays(1);
            var count    = await _orderRepository.GetOrdersCountByDateRangeAsync(dayStart, dayEnd, cancellationToken);
            dailyOrderCounts[dayStart.ToString("yyyy-MM-dd")] = count;
        }

        return new DashboardKpisDto
        {
            TotalOrders           = totalOrders,
            TotalRevenue          = totalRevenue.Amount,
            TotalRevenueCurrency  = totalRevenue.Currency,
            ActivePartners        = activePartners,
            PendingApprovals      = pendingApprovals,
            OrdersToday           = ordersToday,
            RevenueToday          = revenueToday.Amount,
            RevenueTodayCurrency  = revenueToday.Currency,
            OrdersYesterday       = ordersYesterday,
            RevenueYesterday      = revenueYesterday.Amount,
            TotalUsers            = totalUsers,
            DailyOrderCounts      = dailyOrderCounts,
        };
    }
}