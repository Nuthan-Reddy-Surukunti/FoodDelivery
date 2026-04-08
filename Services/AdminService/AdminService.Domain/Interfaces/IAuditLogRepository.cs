using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
    }
}