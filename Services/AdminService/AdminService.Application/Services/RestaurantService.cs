using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AdminService.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        ((Restaurant)restaurant).Approve(request.ApprovalNotes);
        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Log audit trail
        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("Restaurant", id, "Approved", 
                request.ApprovalNotes, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<RestaurantDto> RejectAsync(Guid id, RejectRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        ((Restaurant)restaurant).Reject(request.RejectionReason);
        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Log audit trail
        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("Restaurant", id, "Rejected", 
                request.RejectionReason, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<IEnumerable<RestaurantDto>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var restaurants = await _restaurantRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        return _mapper.Map<IEnumerable<RestaurantDto>>(restaurants);
    }

    public async Task DeleteAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

        await _restaurantRepository.SoftDeleteAsync(restaurantId, cancellationToken);
    }
}
