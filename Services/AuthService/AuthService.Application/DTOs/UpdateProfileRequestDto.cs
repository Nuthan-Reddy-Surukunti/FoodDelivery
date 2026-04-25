namespace AuthService.Application.DTOs;

public class UpdateProfileRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
}
