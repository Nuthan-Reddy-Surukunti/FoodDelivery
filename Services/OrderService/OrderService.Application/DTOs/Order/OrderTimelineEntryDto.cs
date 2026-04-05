namespace OrderService.Application.DTOs.Order;

using OrderService.Domain.Enums;

public class OrderTimelineEntryDto
{
    public OrderStatus Status { get; set; }

    public DateTime OccurredAt { get; set; }

    public string Label { get; set; } = string.Empty;
}