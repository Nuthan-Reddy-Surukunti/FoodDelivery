using AdminService.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminService.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            // Only audit admin actions (POST, PUT, DELETE on admin endpoints)
            if (ShouldAudit(context))
            {
                await AuditRequest(context, auditService);
            }

            await _next(context);
        }

        private static bool ShouldAudit(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value?.ToLowerInvariant();

            // Only audit modifying operations
            if (method != "POST" && method != "PUT" && method != "DELETE")
                return false;

            // Only audit admin endpoints
            if (path == null || !path.Contains("/api/"))
                return false;

            // Skip health checks and similar
            if (path.Contains("/health") || path.Contains("/metrics"))
                return false;

            return true;
        }

        private async Task AuditRequest(HttpContext context, IAuditService auditService)
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated == true)
                return;

            var userId = GetUserId(user);
            var userName = GetUserName(user);

            if (userId == null || string.IsNullOrEmpty(userName))
                return;

            var action = DetermineAction(context.Request.Method, context.Request.Path);
            var entityInfo = ExtractEntityInfo(context.Request.Path);

            if (entityInfo.EntityType == null)
                return;

            var ipAddress = GetClientIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // For POST/PUT requests, capture request body
            object? requestData = null;
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                requestData = await CaptureRequestBody(context.Request);
            }

            try
            {
                await auditService.LogActionAsync(
                    userId.Value,
                    userName,
                    action,
                    entityInfo.EntityType,
                    entityInfo.EntityId ?? Guid.Empty,
                    null, // oldValues - would need to be captured before the action
                    requestData,
                    ipAddress,
                    userAgent
                );
            }
            catch
            {
                // Don't fail the request if audit logging fails
                // Consider adding structured logging here
            }
        }

        private static Guid? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             user.FindFirst("sub")?.Value ??
                             user.FindFirst("user_id")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private static string GetUserName(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ??
                   user.FindFirst("name")?.Value ??
                   user.FindFirst("preferred_username")?.Value ??
                   user.Identity?.Name ??
                   "Unknown Admin";
        }

        private static string DetermineAction(string method, PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";

            if (pathValue.Contains("/approve"))
                return "Approved";
            if (pathValue.Contains("/reject"))
                return "Rejected";
            if (pathValue.Contains("/status"))
                return "StatusChanged";

            return method switch
            {
                "POST" => "Created",
                "PUT" => "Updated",
                "DELETE" => "Deleted",
                _ => "Modified"
            };
        }

        private static (string? EntityType, Guid? EntityId) ExtractEntityInfo(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? "";

            // Extract entity type from path
            string? entityType = null;
            if (pathValue.Contains("/restaurants"))
                entityType = "Restaurant";
            else if (pathValue.Contains("/menu-items"))
                entityType = "MenuItem";
            else if (pathValue.Contains("/orders"))
                entityType = "Order";
            else if (pathValue.Contains("/reports"))
                entityType = "Report";

            // Try to extract entity ID from path (assumes GUID format)
            Guid? entityId = null;
            var segments = pathValue.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (Guid.TryParse(segment, out var id))
                {
                    entityId = id;
                    break;
                }
            }

            return (entityType, entityId);
        }

        private static string? GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        private static async Task<object?> CaptureRequestBody(HttpRequest request)
        {
            if (request.ContentLength == 0 || request.ContentLength > 1024 * 1024) // Skip large bodies
                return null;

            try
            {
                request.EnableBuffering();
                request.Body.Position = 0;

                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var bodyContent = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(bodyContent))
                    return null;

                // Try to parse as JSON
                return JsonSerializer.Deserialize<object>(bodyContent);
            }
            catch
            {
                return null; // If we can't parse, don't include the body
            }
        }
    }
}