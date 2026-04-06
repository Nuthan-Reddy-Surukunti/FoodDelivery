using Microsoft.EntityFrameworkCore;
using AdminService.Infrastructure.Persistence;
using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Enums;

namespace AdminService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AdminServiceDbContext _context;

    public ReportRepository(AdminServiceDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<object>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports.ToListAsync(cancellationToken);
    }

    public async Task<object> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        var report = (Report)entity;
        await _context.Reports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task UpdateAsync(object entity, CancellationToken cancellationToken = default)
    {
        var report = (Report)entity;
        _context.Reports.Update(report);
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

    public async Task<IEnumerable<object>> GetByTypeAsync(ReportType reportType, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.Type == reportType)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<object>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.GeneratedAt >= startDate && r.GeneratedAt <= endDate)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReportMetrics> GetSalesMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalRevenue = orders.Any() 
            ? Money.Create(orders.Sum(o => o.TotalAmount.Amount), orders.First().TotalAmount.Currency)
            : Money.Zero("USD");
        
        var customerIds = orders.Select(o => o.CustomerId).Distinct().Count();
        var restaurantIds = orders.Select(o => o.RestaurantId).Distinct().Count();
        var avgOrderValue = totalOrders > 0 ? (double)totalRevenue.Amount / totalOrders : 0;

        return ReportMetrics.Create(totalOrders, totalRevenue, customerIds, restaurantIds, avgOrderValue, startDate, endDate);
    }

    public async Task<ReportMetrics> GetRestaurantPerformanceAsync(Guid restaurantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.RestaurantId == restaurantId && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalRevenue = orders.Any()
            ? Money.Create(orders.Sum(o => o.TotalAmount.Amount), orders.First().TotalAmount.Currency)
            : Money.Zero("USD");

        var avgOrderValue = totalOrders > 0 ? (double)totalRevenue.Amount / totalOrders : 0;

        return ReportMetrics.Create(totalOrders, totalRevenue, 0, 1, avgOrderValue, startDate, endDate);
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

        var analytics = new Dictionary<string, object>
        {
            { "TotalRegistrations", totalRegistrations },
            { "ActiveUsers", activeUsers },
            { "UsersByRole", new Dictionary<string, int> { { "Customer", totalRegistrations } } }
        };

        return analytics;
    }

    public async Task<Dictionary<string, object>> GetRestaurantAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var restaurants = await _context.Restaurants.ToListAsync(cancellationToken);
        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalRestaurants = restaurants.Count;
        var pendingCount = restaurants.Count(r => r.Status.ToString() == "Pending");
        var approvedCount = restaurants.Count(r => r.Status.ToString() == "Approved");
        var totalRevenue = orders.Any() 
            ? orders.Sum(o => o.TotalAmount.Amount)
            : 0m;

        var revenueByRestaurant = new Dictionary<Guid, decimal>();
        foreach (var restaurantId in orders.Select(o => o.RestaurantId).Distinct())
        {
            revenueByRestaurant[restaurantId] = orders
                .Where(o => o.RestaurantId == restaurantId)
                .Sum(o => o.TotalAmount.Amount);
        }

        var analytics = new Dictionary<string, object>
        {
            { "TotalRestaurants", totalRestaurants },
            { "PendingApprovals", pendingCount },
            { "ApprovedCount", approvedCount },
            { "TotalRevenue", totalRevenue },
            { "RevenueByRestaurant", revenueByRestaurant }
        };

        return analytics;
    }
}
