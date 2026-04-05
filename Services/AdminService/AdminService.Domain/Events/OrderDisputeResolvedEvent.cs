using AdminService.Domain.Enums;

namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when an order dispute is resolved by an admin
/// </summary>
public sealed class OrderDisputeResolvedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid OrderId { get; }
    public DisputeStatus Resolution { get; }
    public string ResolutionNotes { get; }
    public decimal? RefundAmount { get; }

    public OrderDisputeResolvedEvent(
        Guid orderId, 
        DisputeStatus resolution, 
        string resolutionNotes,
        decimal? refundAmount = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        OrderId = orderId;
        Resolution = resolution;
        ResolutionNotes = resolutionNotes ?? throw new ArgumentNullException(nameof(resolutionNotes));
        RefundAmount = refundAmount;
    }
}
