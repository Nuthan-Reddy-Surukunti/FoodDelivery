using Microsoft.EntityFrameworkCore;
using AdminService.Infrastructure.Persistence;
using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AdminServiceDbContext _context;

    public ReportRepository(AdminServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports.ToListAsync(cancellationToken);
    }

    public async Task<Report> AddAsync(Report entity, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Report entity, CancellationToken cancellationToken = default)
    {
        _context.Reports.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
        if (report != null)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByTypeAsync(ReportType reportType, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.Type == reportType)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.GeneratedAt >= startDate && r.GeneratedAt <= endDate)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int TotalOrders, decimal TotalRevenue, string Currency, int TotalCustomers, int TotalRestaurants, double AverageOrderValue)> GetSalesMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var currency = orders.FirstOrDefault()?.Currency ?? "USD";
        var totalCustomers = orders.Select(o => o.CustomerId).Distinct().Count();
        var totalRestaurants = orders.Select(o => o.RestaurantId).Distinct().Count();
        var avgOrderValue = totalOrders > 0 ? (double)totalRevenue / totalOrders : 0;

        return (totalOrders, totalRevenue, currency, totalCustomers, totalRestaurants, avgOrderValue);
    }

    public async Task<Dictionary<string, int>> GetOrderAnalyticsByStatusAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return orders.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<string, object>> GetUserRegistrationAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalRegistrations = orders.Select(o => o.CustomerId).Distinct().Count();
        var activeUsers = orders.GroupBy(o => o.CustomerId).Count();

        return new Dictionary<string, object>
        {
            { "TotalRegistrations", totalRegistrations },
            { "ActiveUsers", activeUsers },
            { "UsersByRole", new Dictionary<string, int> { { "Customer", totalRegistrations } } }
        };
    }

    public async Task<Dictionary<string, object>> GetRestaurantAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var restaurants = await _context.Restaurants.ToListAsync(cancellationToken);
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalRestaurants = restaurants.Count;
        var pendingCount = restaurants.Count(r => r.Status == Domain.Enums.RestaurantStatus.Pending);
        var approvedCount = restaurants.Count(r => r.Status == Domain.Enums.RestaurantStatus.Approved);
        var totalRevenue = orders.Sum(o => o.TotalAmount);

        var revenueByRestaurant = new Dictionary<Guid, decimal>();
        foreach (var restaurantId in orders.Select(o => o.RestaurantId).Distinct())
        {
            revenueByRestaurant[restaurantId] = orders
                .Where(o => o.RestaurantId == restaurantId)
                .Sum(o => o.TotalAmount);
        }

        return new Dictionary<string, object>
        {
            { "TotalRestaurants", totalRestaurants },
            { "PendingApprovals", pendingCount },
            { "ApprovedCount", approvedCount },
            { "TotalRevenue", totalRevenue },
            { "RevenueByRestaurant", revenueByRestaurant }
        };
    }
}
