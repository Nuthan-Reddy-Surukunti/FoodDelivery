namespace AdminService.Application.DTOs.Requests;

/// <summary>
/// Request to approve a menu item
/// </summary>
public class ApproveMenuItemRequest
{
    public string? ApprovalNotes { get; set; }
}