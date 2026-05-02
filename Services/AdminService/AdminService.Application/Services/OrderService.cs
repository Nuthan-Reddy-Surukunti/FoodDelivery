using AutoMapper;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MassTransit;
using QuickBite.Shared.Events.Order;
using Microsoft.Extensions.Logging;

namespace AdminService.Application.Services;

public class OrderService : IOrderService
{
    private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
    {
        { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
        { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Preparing, OrderStatus.Cancelled } },
        { OrderStatus.Preparing, new List<OrderStatus> { OrderStatus.Ready, OrderStatus.Cancelled } },
        { OrderStatus.Ready, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Cancelled } },
        { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
        { OrderStatus.Delivered, new List<OrderStatus>() },
        { OrderStatus.Cancelled, new List<OrderStatus>() }
    };

    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditService _auditService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository, 
        IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, 
        IAuditService auditService,
        IPublishEndpoint publishEndpoint,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _auditService = auditService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {id} not found");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<List<OrderDto>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        OrderStatus? orderStatus = null;
        if (status != null && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
        {
            orderStatus = parsedStatus;
        }

        var orders = await _orderRepository.GetAllAsync(orderStatus, cancellationToken);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string reason, decimal? refundAmount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin order status update requested for {OrderId} to {NewStatus}.", orderId, newStatus);

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found");

        // Capture old status for audit logging
        var oldStatus = order.Status;

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        var adminUserId = Guid.TryParse(adminUserIdClaim, out var userId) ? userId : Guid.Empty;

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for admin status changes", nameof(reason));

        if (refundAmount.HasValue && refundAmount.Value > order.TotalAmount)
            throw new ArgumentException("Refund amount cannot exceed order total", nameof(refundAmount));

        if (!IsTransitionAllowed(order.Status, newStatus) && reason.Length < 10)
        {
            _logger.LogWarning(
                "Admin order status update rejected for {OrderId}. CurrentStatus={CurrentStatus}, RequestedStatus={RequestedStatus}.",
                orderId,
                order.Status,
                newStatus);
            throw new ArgumentException("Admin override requires detailed reason (min 10 characters)", nameof(reason));
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        // Save changes
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Log audit trail
        if (adminUserId != Guid.Empty)
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogStatusChangeAsync(orderId, oldStatus.ToString(), newStatus.ToString(), 
                reason, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }

        // 🚀 Publish event to notify other services (OrderService, CatalogService, etc.)
        await _publishEndpoint.Publish(new OrderStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            OrderId = orderId,
            UserId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            OldStatus = oldStatus.ToString(),
            NewStatus = newStatus.ToString(),
            StatusReason = reason
        }, cancellationToken);

        _logger.LogInformation(
            "Admin order status update completed for {OrderId}. PreviousStatus={PreviousStatus}, NewStatus={NewStatus}.",
            orderId,
            oldStatus,
            newStatus);

        return _mapper.Map<OrderDto>(order);
    }

    private static bool IsTransitionAllowed(OrderStatus fromStatus, OrderStatus toStatus)
    {
        return AllowedTransitions.TryGetValue(fromStatus, out var nextStatuses) && nextStatuses.Contains(toStatus);
    }
}
