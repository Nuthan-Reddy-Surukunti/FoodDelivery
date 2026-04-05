namespace AuthService.Application.DTOs;

public class AdminApprovalDto
{
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
}

public class AdminRejectionDto
{
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
