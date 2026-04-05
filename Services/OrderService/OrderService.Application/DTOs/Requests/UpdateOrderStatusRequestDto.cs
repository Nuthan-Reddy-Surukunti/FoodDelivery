namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class UpdateOrderStatusRequestDto
{
    public Guid OrderId { get; set; }

    public OrderStatus TargetStatus { get; set; }

    public decimal? RefundAmount { get; set; }
}