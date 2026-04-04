namespace CatalogService.Application.DTOs.OperatingHours;

public class OperatingHoursDto
{
    public Guid Id { get; set; }

    public string DayOfWeek { get; set; } = string.Empty;

    public TimeSpan OpenTime { get; set; }

    public TimeSpan CloseTime { get; set; }

    public bool IsClosed { get; set; }
}
