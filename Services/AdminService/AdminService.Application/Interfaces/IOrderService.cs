using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Domain.Enums;

namespace AdminService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<OrderDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string reason, decimal? refundAmount, CancellationToken cancellationToken = default);
}
