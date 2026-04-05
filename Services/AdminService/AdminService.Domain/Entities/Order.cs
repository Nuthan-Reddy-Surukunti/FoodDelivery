using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Events;

namespace AdminService.Domain.Entities;

/// <summary>
/// Order aggregate root representing customer orders
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DisputeStatus? DisputeStatus { get; private set; }
    public string? DisputeReason { get; private set; }
    public string? DisputeResolutionNotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? DisputeRaisedAt { get; private set; }
    public DateTime? DisputeResolvedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() 
    {
        TotalAmount = null!;
    } // For EF Core

    private Order(Guid customerId, Guid restaurantId, Money totalAmount)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        RestaurantId = restaurantId;
        TotalAmount = totalAmount ?? throw new ArgumentNullException(nameof(totalAmount));
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Order Create(Guid customerId, Guid restaurantId, Money totalAmount)
    {
        return new Order(customerId, restaurantId, totalAmount);
    }

    public void RaiseDispute(string reason)
    {
        if (Status != OrderStatus.Delivered && Status != OrderStatus.OutForDelivery)
            throw new InvalidOperationException("Can only raise dispute for delivered or in-transit orders");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Dispute reason is required", nameof(reason));

        Status = OrderStatus.Disputed;
        DisputeStatus = Enums.DisputeStatus.Open;
        DisputeReason = reason;
        DisputeRaisedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResolveDispute(DisputeStatus resolution, string resolutionNotes, decimal? refundAmount = null)
    {
        if (Status != OrderStatus.Disputed)
            throw new InvalidOperationException("Order is not in disputed status");

        if (string.IsNullOrWhiteSpace(resolutionNotes))
            throw new ArgumentException("Resolution notes are required", nameof(resolutionNotes));

        DisputeStatus = resolution;
        DisputeResolutionNotes = resolutionNotes;
        DisputeResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Return to delivered status after resolution
        Status = OrderStatus.Delivered;

        AddDomainEvent(new OrderDisputeResolvedEvent(Id, resolution, resolutionNotes, refundAmount));
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
        }
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
