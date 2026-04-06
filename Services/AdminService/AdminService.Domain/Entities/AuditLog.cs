using System;
using System.Text.Json;

namespace AdminService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public string Action { get; private set; } = string.Empty;
        public string EntityType { get; private set; } = string.Empty;
        public Guid EntityId { get; private set; }
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string? IPAddress { get; private set; }
        public string? UserAgent { get; private set; }

        private AuditLog() { }

        private AuditLog(Guid userId, string userName, string action, string entityType, 
            Guid entityId, object? oldValues = null, object? newValues = null, 
            string? ipAddress = null, string? userAgent = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            EntityId = entityId;
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null;
            Timestamp = DateTime.UtcNow;
            IPAddress = ipAddress;
            UserAgent = userAgent;
        }

        public static AuditLog Create(Guid userId, string userName, string action, 
            string entityType, Guid entityId, object? oldValues = null, object? newValues = null, 
            string? ipAddress = null, string? userAgent = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("UserName is required", nameof(userName));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action is required", nameof(action));

            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required", nameof(entityType));

            if (entityId == Guid.Empty)
                throw new ArgumentException("EntityId cannot be empty", nameof(entityId));

            return new AuditLog(userId, userName, action, entityType, entityId, 
                oldValues, newValues, ipAddress, userAgent);
        }
    }
}