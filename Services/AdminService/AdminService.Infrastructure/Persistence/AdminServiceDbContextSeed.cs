using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;

namespace AdminService.Infrastructure.Persistence;

public static class AdminServiceDbContextSeed
{
    public static async Task SeedAsync(AdminServiceDbContext context)
    {
        // Seed data will be added here when needed
        await Task.CompletedTask;
    }
}
