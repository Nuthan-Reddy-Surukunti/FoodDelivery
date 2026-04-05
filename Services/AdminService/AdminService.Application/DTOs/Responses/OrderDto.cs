namespace AdminService.Application.DTOs.Responses;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? DisputeStatus { get; set; }
    public string? DisputeReason { get; set; }
    public string? DisputeResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? DisputeRaisedAt { get; set; }
    public DateTime? DisputeResolvedAt { get; set; }
}
