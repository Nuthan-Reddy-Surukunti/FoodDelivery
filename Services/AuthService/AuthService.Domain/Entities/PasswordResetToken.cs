using System;

namespace AuthService.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }=Guid.NewGuid();
    public string Token { get; set; }=string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool IsUsed { get; set; }=false;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
