using Microsoft.EntityFrameworkCore;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Persistence.Configurations;

namespace AdminService.Infrastructure.Persistence;

public class AdminServiceDbContext : DbContext
{
    public AdminServiceDbContext(DbContextOptions<AdminServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new ReportConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Clear domain events before saving
        var restaurantEntities = ChangeTracker.Entries<Restaurant>().Select(e => e.Entity).ToList();
        var orderEntities = ChangeTracker.Entries<Order>().Select(e => e.Entity).ToList();
        var reportEntities = ChangeTracker.Entries<Report>().Select(e => e.Entity).ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after successful save

        return result;
    }
}
