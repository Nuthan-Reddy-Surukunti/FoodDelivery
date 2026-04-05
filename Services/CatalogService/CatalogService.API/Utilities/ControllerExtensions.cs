using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Utilities;

/// <summary>
/// Extension methods for controllers to extract user information from JWT claims.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Gets the current user's ID from JWT claims.
    /// </summary>
    public static Guid GetCurrentUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Gets the current user's role from JWT claims.
    /// </summary>
    public static string GetCurrentUserRole(this ControllerBase controller)
    {
        return controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Unknown";
    }
}
