using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        }

        public async Task LogActionAsync(Guid userId, string userName, string action, string entityType, 
            Guid entityId, object? oldValues = null, object? newValues = null, 
            string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.Create(userId, userName, action, entityType, entityId, 
                oldValues, newValues, ipAddress, userAgent);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        }

        public async Task LogStatusChangeAsync(Guid orderId, string oldStatus, string newStatus, 
            string reason, Guid adminUserId, string adminUserName, string? ipAddress = null, 
            string? userAgent = null, CancellationToken cancellationToken = default)
        {
            var oldValues = new { Status = oldStatus };
            var newValues = new { Status = newStatus, Reason = reason };

            await LogActionAsync(adminUserId, adminUserName, "StatusChanged", "Order", orderId, 
                oldValues, newValues, ipAddress, userAgent, cancellationToken);
        }

        public async Task LogApprovalActionAsync(string entityType, Guid entityId, string action, 
            string? notes, Guid adminUserId, string adminUserName, string? ipAddress = null, 
            string? userAgent = null, CancellationToken cancellationToken = default)
        {
            var values = new { Action = action, Notes = notes, Timestamp = DateTime.UtcNow };

            await LogActionAsync(adminUserId, adminUserName, action, entityType, entityId, 
                null, values, ipAddress, userAgent, cancellationToken);
        }
    }
}