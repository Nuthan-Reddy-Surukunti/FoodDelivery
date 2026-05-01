namespace AdminService.Application.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

public class DeactivateRestaurantRequest
{
    [Required]
    [StringLength(500, MinimumLength = 2)]
    public string Reason { get; set; } = string.Empty;
}
