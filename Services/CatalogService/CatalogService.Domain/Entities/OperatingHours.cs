namespace CatalogService.Domain.Entities;

using CatalogService.Domain.Common;

public class OperatingHours : BaseEntity
{
    public Guid RestaurantId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan OpenTime { get; set; }

    public TimeSpan CloseTime { get; set; }

    public bool IsClosed { get; set; } = false;

    public Restaurant? Restaurant { get; set; }
}
