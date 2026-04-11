using AdminService.Domain.Enums;

namespace AdminService.Domain.Entities;

/// <summary>
/// Order aggregate root representing customer orders
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public Guid? SyncEventId { get; set; }
}
