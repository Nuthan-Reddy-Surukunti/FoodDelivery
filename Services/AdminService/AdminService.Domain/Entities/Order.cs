using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Events;
using AdminService.Domain.Services;

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
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

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

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
        }
    }

    public void UpdateStatusWithAdmin(OrderStatus newStatus, string reason, string adminUserId, decimal? refundAmount = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for admin status changes", nameof(reason));

        if (refundAmount.HasValue && refundAmount.Value > TotalAmount.Amount)
            throw new ArgumentException("Refund amount cannot exceed order total", nameof(refundAmount));

        var previousStatus = Status;
        
        // Validate transition using business rules
        if (!OrderStatusTransitionService.IsTransitionAllowed(Status, newStatus))
        {
            // Admin override is required - log this change with extra detail
            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 10)
                throw new ArgumentException("Admin override requires detailed reason (min 10 characters)", nameof(reason));
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
        {
            DeliveredAt = DateTime.UtcNow;
        }

        // Create domain event for audit trail
        var statusChangedEvent = new OrderStatusChangedEvent(
            Id, 
            previousStatus, 
            newStatus, 
            reason, 
            adminUserId,
            refundAmount
        );
        
        AddDomainEvent(statusChangedEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
