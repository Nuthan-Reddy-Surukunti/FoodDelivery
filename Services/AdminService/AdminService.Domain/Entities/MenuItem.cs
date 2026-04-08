using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

/// <summary>
/// Represents a menu item that requires moderation
/// </summary>
public class MenuItem
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CategoryId { get; set; }
    public MenuItemStatus Status { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}