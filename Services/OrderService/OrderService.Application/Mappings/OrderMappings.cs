namespace OrderService.Application.Mappings;

using OrderService.Application.DTOs.Common;
using OrderService.Application.DTOs.Delivery;
using OrderService.Application.DTOs.Order;
using OrderService.Application.DTOs.Payment;
using OrderService.Domain.Entities;

public static class OrderMappings
{
    public static OrderDetailDto MapToDto(Order order)
    {
        var subtotal = CalculateOrderSubtotal(order);
        var total = CalculateOrderTotal(order);

        // Determine delivery assignment status and message
        string? deliveryAssignmentStatus = null;
        string? deliveryAssignmentMessage = null;

        if (order.DeliveryAssignment is not null)
        {
            deliveryAssignmentStatus = "assigned";
        }
        else
        {
            deliveryAssignmentStatus = "pending_agents";
            deliveryAssignmentMessage = "No delivery agents available at this time. Please try again after some time.";
        }

        return new OrderDetailDto
        {
            OrderId = order.Id,
            UserId = order.UserId,
            RestaurantId = order.RestaurantId,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Subtotal = subtotal,
            Total = total,
            Currency = "USD",
            DeliveryAddress = MapDeliveryAddress(order),
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                OrderItemId = item.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                UnitPriceSnapshot = item.UnitPrice,
                CustomizationNotes = item.CustomizationNotes,
                Subtotal = item.Quantity * item.UnitPrice
            }).ToList(),
            Payment = order.Payment is null ? null : MapPayment(order.Payment),
            DeliveryAssignment = order.DeliveryAssignment is null ? null : MapDeliveryAssignment(order.DeliveryAssignment),
            DeliveryAssignmentStatus = deliveryAssignmentStatus,
            DeliveryAssignmentMessage = deliveryAssignmentMessage
        };
    }

    private static decimal CalculateOrderSubtotal(Order order)
    {
        return order.OrderItems.Sum(item => item.Quantity * item.UnitPrice);
    }

    private static decimal CalculateOrderTotal(Order order)
    {
        return order.TotalAmount;
    }

    private static AddressDto? MapDeliveryAddress(Order order)
    {
        if (string.IsNullOrEmpty(order.DeliveryAddressLine1))
        {
            return null;
        }

        return new AddressDto
        {
            Street = order.DeliveryAddressLine1,
            City = order.DeliveryCity ?? string.Empty,
            Pincode = order.DeliveryPostalCode ?? string.Empty,
            Latitude = order.DeliveryLatitude ?? 0,
            Longitude = order.DeliveryLongitude ?? 0,
            AddressType = 0
        };
    }

    private static PaymentDto? MapPayment(Payment payment)
    {
        return new PaymentDto
        {
            PaymentId = payment.Id,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            Amount = payment.Amount,
            Currency = "USD",
            RefundedAmount = payment.RefundedAmount,
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            ProcessedAt = payment.ProcessedAt
        };
    }

    private static DeliveryAssignmentDto? MapDeliveryAssignment(DeliveryAssignment assignment)
    {
        return new DeliveryAssignmentDto
        {
            DeliveryAssignmentId = assignment.Id,
            DeliveryAgentId = assignment.DeliveryAgentId,
            AgentAuthUserId = assignment.DeliveryAgent?.AuthUserId,
            AssignedAt = assignment.AssignedAt,
            PickedUpAt = assignment.PickedUpAt,
            DeliveredAt = assignment.DeliveredAt,
            CurrentStatus = assignment.CurrentStatus,
            AgentName = assignment.DeliveryAgent?.FullName,
            AgentEmail = assignment.DeliveryAgent?.Email,
            AgentPhone = assignment.DeliveryAgent?.PhoneNumber
        };
    }
}
