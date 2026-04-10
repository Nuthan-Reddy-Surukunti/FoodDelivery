namespace OrderService.Application.DTOs.Order;

using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Payment;
using OrderService.Domain.Enums;

public class OrderDetailDto
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public AddressDto? DeliveryAddress { get; set; }

    public List<OrderItemDto> Items { get; set; } = [];

    public PaymentDto? Payment { get; set; }

    public DeliveryAssignmentDto? DeliveryAssignment { get; set; }

    public string? DeliveryAssignmentStatus { get; set; }

    public string? DeliveryAssignmentMessage { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; } = "INR";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}