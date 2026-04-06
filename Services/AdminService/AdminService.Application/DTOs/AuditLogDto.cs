using System;

namespace AdminService.Application.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class AuditLogFilterRequest
    {
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}