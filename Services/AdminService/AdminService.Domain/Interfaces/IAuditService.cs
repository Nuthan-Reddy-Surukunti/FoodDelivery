using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Domain.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(Guid userId, string userName, string action, string entityType, 
            Guid entityId, object? oldValues = null, object? newValues = null, 
            string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

        Task LogStatusChangeAsync(Guid orderId, string oldStatus, string newStatus, 
            string reason, Guid adminUserId, string adminUserName, string? ipAddress = null, 
            string? userAgent = null, CancellationToken cancellationToken = default);

        Task LogApprovalActionAsync(string entityType, Guid entityId, string action, 
            string? notes, Guid adminUserId, string adminUserName, string? ipAddress = null, 
            string? userAgent = null, CancellationToken cancellationToken = default);
    }
}