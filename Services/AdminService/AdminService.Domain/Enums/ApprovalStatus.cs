namespace AdminService.Domain.Enums;

/// <summary>
/// Represents the approval status of content requiring moderation
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Content is pending admin approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Content has been approved by admin
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Content was rejected by admin
    /// </summary>
    Rejected = 3
}