using System;

namespace AuthService.Domain.Entities;

public class PasswordResetToken
{
    public string Id { get; set; }=string.Empty;
    public string Token { get; set; }=string.Empty;
    public string UserId { get; set; }=string.Empty;
    public DateTime ExpiredAt { get; set; }
    public bool IsUsed { get; set; }=false;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
