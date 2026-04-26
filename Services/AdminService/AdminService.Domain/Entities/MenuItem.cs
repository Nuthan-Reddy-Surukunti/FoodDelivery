namespace AdminService.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsVeg { get; set; }
    public string AvailabilityStatus { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public Guid? SyncEventId { get; set; }

    public Restaurant? Restaurant { get; set; }
}
