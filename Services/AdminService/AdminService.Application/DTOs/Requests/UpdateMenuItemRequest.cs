namespace AdminService.Application.DTOs.Requests;

/// <summary>
/// Request to update an existing menu item
/// </summary>
public class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? CategoryId { get; set; }
}