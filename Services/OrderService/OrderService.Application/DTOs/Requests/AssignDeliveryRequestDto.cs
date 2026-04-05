namespace OrderService.Application.DTOs.Requests;

public class AssignDeliveryRequestDto
{
    public Guid OrderId { get; set; }

    public Guid DeliveryAgentId { get; set; }
}