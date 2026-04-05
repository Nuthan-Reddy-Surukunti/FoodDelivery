using Microsoft.EntityFrameworkCore;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Persistence;

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
            .Where(r => r.StartDate >= startDate && r.EndDate <= endDate)
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

    public async Task<ReportMetrics> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var totalCustomers = await _context.Users
            .Where(u => u.Role == UserRole.Customer && u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var totalRestaurants = await _context.Restaurants
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        var totalOrders = await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync(cancellationToken);

        return ReportMetrics.Create(totalOrders, Money.Zero("USD"), totalCustomers, totalRestaurants, 0, startDate, endDate);
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
}
