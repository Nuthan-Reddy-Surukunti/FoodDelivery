using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
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


}
