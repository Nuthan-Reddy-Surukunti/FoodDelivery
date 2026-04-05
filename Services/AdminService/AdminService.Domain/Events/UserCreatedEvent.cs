using AdminService.Domain.Enums;

namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a new user is created in the system
/// </summary>
public sealed class UserCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid UserId { get; }
    public string Email { get; }
    public UserRole Role { get; }

    public UserCreatedEvent(Guid userId, string email, UserRole role)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        UserId = userId;
        Email = email;
        Role = role;
    }
}
