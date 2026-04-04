using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Data;

namespace CatalogService.Infrastructure.Persistence.Seeding;

public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext context)
    {
        // Check if restaurants already exist to avoid re-seeding
        if (context.Restaurants.Any())
            return;

        // TODO: Add sample restaurants, categories, menu items, and operating hours
        // User will provide seeding data in future

        await context.SaveChangesAsync();
    }
}
