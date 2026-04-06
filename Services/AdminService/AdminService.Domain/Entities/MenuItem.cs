using AdminService.Domain.Enums;
using AdminService.Domain.Events;
using AdminService.Domain.ValueObjects;

namespace AdminService.Domain.Entities;

/// <summary>
/// Represents a menu item that requires moderation
/// </summary>
public class MenuItem
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public string? CategoryId { get; private set; }
    public MenuItemStatus Status { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public string? ApprovalNotes { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private parameterless constructor for EF Core
    private MenuItem() 
    { 
        Name = null!;
        Description = null!;
        Price = null!;
    }

    private MenuItem(Guid restaurantId, string name, string description, Money price, string? categoryId = null)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CategoryId = categoryId;
        Status = MenuItemStatus.Inactive; // Start as inactive until activated
        ApprovalStatus = ApprovalStatus.Pending; // Require approval for new items
        CreatedAt = DateTime.UtcNow;
        
        // Validate business rules
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        
        AddDomainEvent(new MenuItemCreatedEvent(Id, RestaurantId, Name));
    }

    /// <summary>
    /// Creates a new menu item
    /// </summary>
    public static MenuItem Create(Guid restaurantId, string name, string description, Money price, string? categoryId = null)
    {
        if (restaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID cannot be empty", nameof(restaurantId));

        return new MenuItem(restaurantId, name, description, price, categoryId);
    }

    /// <summary>
    /// Updates the menu item details
    /// </summary>
    public void UpdateDetails(string? name = null, string? description = null, Money? price = null, string? categoryId = null)
    {
        if (ApprovalStatus == ApprovalStatus.Rejected)
            throw new InvalidOperationException("Cannot update rejected menu item. Create a new one instead.");

        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
        {
            ValidateName(name);
            Name = name;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(description) && description != Description)
        {
            ValidateDescription(description);
            Description = description;
            hasChanges = true;
        }

        if (price != null && price != Price)
        {
            ValidatePrice(price);
            Price = price;
            hasChanges = true;
        }

        if (categoryId != CategoryId)
        {
            CategoryId = categoryId;
            hasChanges = true;
        }

        if (hasChanges)
        {
            // If item was approved and now modified, reset to pending
            if (ApprovalStatus == ApprovalStatus.Approved)
            {
                ApprovalStatus = ApprovalStatus.Pending;
                ApprovedBy = null;
                ApprovedAt = null;
                ApprovalNotes = null;
            }

            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Approves the menu item for content moderation
    /// </summary>
    public void Approve(string approvedBy, string? approvalNotes = null)
    {
        if (ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve menu item with approval status: {ApprovalStatus}");

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approved by is required", nameof(approvedBy));

        ApprovalStatus = ApprovalStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = approvalNotes;
        RejectionReason = null;
        RejectedBy = null;
        RejectedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemApprovedEvent(Id, RestaurantId, Name, approvedBy, approvalNotes));
    }

    /// <summary>
    /// Rejects the menu item for content moderation
    /// </summary>
    public void Reject(string rejectedBy, string rejectionReason)
    {
        if (ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject menu item with approval status: {ApprovalStatus}");

        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejected by is required", nameof(rejectedBy));

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(rejectionReason));

        ApprovalStatus = ApprovalStatus.Rejected;
        RejectedBy = rejectedBy;
        RejectionReason = rejectionReason;
        RejectedAt = DateTime.UtcNow;
        Status = MenuItemStatus.Inactive; // Auto-deactivate rejected items
        ApprovalNotes = null;
        ApprovedBy = null;
        ApprovedAt = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemRejectedEvent(Id, RestaurantId, Name, rejectedBy, rejectionReason));
    }

    /// <summary>
    /// Activates the menu item (makes it available for ordering)
    /// </summary>
    public void Activate()
    {
        if (ApprovalStatus != ApprovalStatus.Approved)
            throw new InvalidOperationException("Cannot activate menu item that is not approved");

        if (Status == MenuItemStatus.Active)
            return; // Already active

        Status = MenuItemStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the menu item (makes it unavailable for ordering)
    /// </summary>
    public void Deactivate()
    {
        if (Status == MenuItemStatus.Inactive)
            return; // Already inactive

        Status = MenuItemStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the price of the menu item
    /// </summary>
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice == null)
            throw new ArgumentNullException(nameof(newPrice));

        if (ApprovalStatus == ApprovalStatus.Rejected)
            throw new InvalidOperationException("Cannot update price of rejected menu item");

        ValidatePrice(newPrice);

        if (newPrice != Price)
        {
            Price = newPrice;

            // If item was approved and price changed, reset to pending
            if (ApprovalStatus == ApprovalStatus.Approved)
            {
                ApprovalStatus = ApprovalStatus.Pending;
                ApprovedBy = null;
                ApprovedAt = null;
                ApprovalNotes = null;
            }

            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Checks if the menu item can be ordered (active and approved)
    /// </summary>
    public bool CanBeOrdered() => Status == MenuItemStatus.Active && ApprovalStatus == ApprovalStatus.Approved;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Menu item name cannot be empty", nameof(name));

        if (name.Length > 255)
            throw new ArgumentException("Menu item name cannot exceed 255 characters", nameof(name));

        if (name.Trim() != name)
            throw new ArgumentException("Menu item name cannot have leading or trailing whitespace", nameof(name));
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Menu item description cannot be empty", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Menu item description cannot exceed 1000 characters", nameof(description));
    }

    private static void ValidatePrice(Money price)
    {
        if (price.Amount <= 0)
            throw new ArgumentException("Menu item price must be greater than zero", nameof(price));

        if (price.Amount > 10000) // Reasonable upper limit
            throw new ArgumentException("Menu item price cannot exceed 10,000", nameof(price));
    }

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}