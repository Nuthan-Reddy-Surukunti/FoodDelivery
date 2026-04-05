using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;

namespace AdminService.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<RestaurantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<PagedResultDto<RestaurantDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        RestaurantStatus? restaurantStatus = null;
        if (status != null && Enum.TryParse<RestaurantStatus>(status, out var parsedStatus))
        {
            restaurantStatus = parsedStatus;
        }

        var (restaurants, totalCount) = await _restaurantRepository.GetPagedAsync(pageNumber, pageSize, restaurantStatus, cancellationToken);
        
        return new PagedResultDto<RestaurantDto>
        {
            Items = _mapper.Map<IEnumerable<RestaurantDto>>(restaurants),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<RestaurantDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var restaurants = await _restaurantRepository.GetPendingApprovalsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RestaurantDto>>(restaurants);
    }

    public async Task<RestaurantDto> ApproveAsync(Guid id, ApproveRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        ((Restaurant)restaurant).Approve(request.ApprovalNotes);
        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<RestaurantDto> RejectAsync(Guid id, RejectRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        ((Restaurant)restaurant).Reject(request.RejectionReason);
        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        return _mapper.Map<RestaurantDto>(restaurant);
    }
}
