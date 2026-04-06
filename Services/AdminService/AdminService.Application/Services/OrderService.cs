using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AdminService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditService _auditService;

    public OrderService(IOrderRepository orderRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuditService auditService)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _auditService = auditService;
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {id} not found");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResultDto<OrderDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        OrderStatus? orderStatus = null;
        if (status != null && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
        {
            orderStatus = parsedStatus;
        }

        var (orders, totalCount) = await _orderRepository.GetPagedAsync(pageNumber, pageSize, orderStatus, cancellationToken);
        
        return new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<IEnumerable<OrderDto>>(orders),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string reason, decimal? refundAmount, CancellationToken cancellationToken = default)
    {
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

        // Update status with admin override
        order.UpdateStatusWithAdmin(newStatus, reason, adminUserIdClaim ?? "system", refundAmount);

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

        return _mapper.Map<OrderDto>(order);
    }
}
