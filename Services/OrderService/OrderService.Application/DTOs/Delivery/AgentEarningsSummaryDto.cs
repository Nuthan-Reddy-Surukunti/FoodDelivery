namespace OrderService.Application.DTOs.Delivery;

public class AgentEarningsSummaryDto
{
    public int TotalDeliveries { get; set; }
    public int TodayDeliveries { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal TodayOrderValue { get; set; }
    public List<AgentDeliveryRecordDto> History { get; set; } = [];
}

public class AgentDeliveryRecordDto
{
    public Guid OrderId { get; set; }
    public string? RestaurantId { get; set; }
    public decimal OrderTotal { get; set; }
    public int ItemCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
