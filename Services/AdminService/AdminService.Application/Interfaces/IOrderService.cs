using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;

namespace AdminService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<OrderDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetDisputedOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> ResolveDisputeAsync(Guid id, ResolveDisputeRequest request, CancellationToken cancellationToken = default);
}
