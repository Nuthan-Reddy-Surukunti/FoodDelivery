using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Events;

namespace AdminService.Domain.Entities;

/// <summary>
/// User aggregate root representing system users
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private User() 
    {
        Email = null!;
        PasswordHash = null!;
        ContactInfo = null!;
    } // For EF Core

    private User(string email, string passwordHash, UserRole role, ContactInfo contactInfo)
    {
        Id = Guid.NewGuid();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role;
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string email, string passwordHash, UserRole role, ContactInfo contactInfo)
    {
        var user = new User(email, passwordHash, role, contactInfo);
        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email, user.Role));
        return user;
    }

    public void UpdateContactInfo(ContactInfo newContactInfo)
    {
        ContactInfo = newContactInfo ?? throw new ArgumentNullException(nameof(newContactInfo));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
