namespace AdminService.Application.DTOs.Requests;

/// <summary>
/// Request to reject a menu item
/// </summary>
public class RejectMenuItemRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}