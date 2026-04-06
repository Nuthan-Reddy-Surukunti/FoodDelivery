using AdminService.Domain.Enums;

namespace AdminService.Application.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
}