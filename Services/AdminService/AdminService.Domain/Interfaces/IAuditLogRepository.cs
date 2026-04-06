using AdminService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Domain.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, 
            string? entityType = null, Guid? entityId = null, Guid? userId = null, 
            DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}