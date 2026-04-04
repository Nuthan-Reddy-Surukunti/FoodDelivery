using System;

namespace AuthService.Domain.Entities;

public class EmailVerificationToken
{
    public Guid Id { get; set; }=Guid.NewGuid();
    public Guid UserId { get; set; }
    public string OTP { get; set; }=string.Empty;
    public DateTime ExpiredAt { get; set; }
    public bool IsUsed { get; set; }=false;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
