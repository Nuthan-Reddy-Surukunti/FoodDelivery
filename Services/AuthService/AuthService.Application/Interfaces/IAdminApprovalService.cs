namespace AuthService.Application.Interfaces;

/// <summary>
/// Service for handling admin approval of RestaurantPartner registrations
/// </summary>
public interface IAdminApprovalService
{
    /// <summary>
    /// Get all pending RestaurantPartner approval requests
    /// </summary>
    Task<IEnumerable<dynamic>> GetPendingApprovalsAsync();
    
    /// <summary>
    /// Approve a pending RestaurantPartner account
    /// </summary>
    Task<bool> ApproveUserAsync(Guid userId, Guid approvingAdminId, string? notes = null);
    
    /// <summary>
    /// Reject a pending RestaurantPartner application
    /// </summary>
    Task<bool> RejectUserAsync(Guid userId, Guid rejectingAdminId, string reason);
}
