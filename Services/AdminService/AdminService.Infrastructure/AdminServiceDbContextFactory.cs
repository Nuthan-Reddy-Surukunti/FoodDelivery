using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Infrastructure;

public class AdminServiceDbContextFactory : IDesignTimeDbContextFactory<AdminServiceDbContext>
{
    public AdminServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdminServiceDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=AdminServiceDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=True;");
        return new AdminServiceDbContext(optionsBuilder.Options);
    }
}
