namespace AdminService.Application.DTOs.Responses;

/// <summary>
/// Response DTO for menu item information
/// </summary>
public class MenuItemDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? ApprovalNotes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanBeOrdered { get; set; }
}