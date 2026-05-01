using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class GoogleLoginDto
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
