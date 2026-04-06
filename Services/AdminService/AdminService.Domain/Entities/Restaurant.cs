using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Events;

namespace AdminService.Domain.Entities;

/// <summary>
/// Restaurant aggregate root representing restaurant partners
/// </summary>
public class Restaurant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Address Address { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public RestaurantStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Restaurant() 
    {
        Name = null!;
        Description = null!;
        Address = null!;
        ContactInfo = null!;
    } // For EF Core

    private Restaurant(string name, string description, Address address, ContactInfo contactInfo)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        Status = RestaurantStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Restaurant Create(string name, string description, Address address, ContactInfo contactInfo)
    {
        return new Restaurant(name, description, address, contactInfo);
    }

    public void Approve(string? approvalNotes = null)
    {
        if (Status != RestaurantStatus.Pending)
            throw new InvalidOperationException($"Cannot approve restaurant with status: {Status}");

        Status = RestaurantStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RejectionReason = null;

        AddDomainEvent(new RestaurantApprovedEvent(Id, Name, approvalNotes));
    }

    public void Reject(string rejectionReason)
    {
        if (Status != RestaurantStatus.Pending)
            throw new InvalidOperationException($"Cannot reject restaurant with status: {Status}");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(rejectionReason));

        Status = RestaurantStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RejectionReason = rejectionReason;

        AddDomainEvent(new RestaurantRejectedEvent(Id, Name, rejectionReason));
    }





    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
