using System.Text.Json.Serialization;
using AdminService.Domain.Enums;

namespace AdminService.Application.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus NewStatus { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
}