using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using QuickBite.Shared.Events.Catalog;
using MassTransit;
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
    private readonly IPublishEndpoint _publishEndpoint;

    public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper, IAuditService auditService, IHttpContextAccessor httpContextAccessor, IPublishEndpoint publishEndpoint)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task<RestaurantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<List<RestaurantDto>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        RestaurantStatus? restaurantStatus = null;
        if (status != null && Enum.TryParse<RestaurantStatus>(status, out var parsedStatus))
        {
            restaurantStatus = parsedStatus;
        }

        var restaurants = await _restaurantRepository.GetAllAsync(restaurantStatus, cancellationToken);
        return _mapper.Map<List<RestaurantDto>>(restaurants);
    }

    public async Task<IEnumerable<RestaurantDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var restaurants = await _restaurantRepository.GetPendingApprovalsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<RestaurantDto>>(restaurants);
    }

    public async Task<RestaurantDto> ApproveAsync(Guid id, ApproveRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        if (restaurant.Status != RestaurantStatus.Pending)
            throw new InvalidOperationException($"Cannot approve restaurant with status: {restaurant.Status}");

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        restaurant.Status = RestaurantStatus.Active;
        restaurant.ApprovedAt = DateTime.UtcNow;
        restaurant.UpdatedAt = DateTime.UtcNow;
        restaurant.RejectionReason = null;

        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Log audit trail
        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("Restaurant", id, "Approved", 
                request.ApprovalNotes, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }

        // Publish restaurant approved event
        await _publishEndpoint.Publish(new RestaurantApprovedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = restaurant.Id,
            Name = restaurant.Name,
            ApprovedBy = adminUserName
        }, cancellationToken);

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<RestaurantDto> RejectAsync(Guid id, RejectRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        if (restaurant.Status != RestaurantStatus.Pending)
            throw new InvalidOperationException($"Cannot reject restaurant with status: {restaurant.Status}");

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(request.RejectionReason));

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        restaurant.Status = RestaurantStatus.Inactive;
        restaurant.RejectedAt = DateTime.UtcNow;
        restaurant.UpdatedAt = DateTime.UtcNow;
        restaurant.RejectionReason = request.RejectionReason;

        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Publish restaurant rejected event
        await _publishEndpoint.Publish(new RestaurantRejectedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = restaurant.Id,
            Name = restaurant.Name,
            RejectionReason = request.RejectionReason
        }, cancellationToken);

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

    public async Task<RestaurantDto> DeactivateAsync(Guid id, DeactivateRestaurantRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        if (restaurant.Status != RestaurantStatus.Active)
            throw new InvalidOperationException($"Cannot deactivate restaurant with status: {restaurant.Status}");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Deactivation reason is required", nameof(request.Reason));

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        restaurant.Status = RestaurantStatus.Inactive;
        restaurant.UpdatedAt = DateTime.UtcNow;
        // Re-using RejectionReason field for now, or you could add DeactivationReason to the entity
        restaurant.RejectionReason = request.Reason; 

        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Publish restaurant deactivated event
        await _publishEndpoint.Publish(new RestaurantDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = restaurant.Id,
            Name = restaurant.Name,
            Reason = request.Reason,
            DeactivatedBy = adminUserName
        }, cancellationToken);

        // Log audit trail
        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("Restaurant", id, "Deactivated", 
                request.Reason, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }

        return _mapper.Map<RestaurantDto>(restaurant);
    }

    public async Task<RestaurantDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(id, cancellationToken);
        if (restaurant == null)
            throw new KeyNotFoundException($"Restaurant with ID {id} not found");

        if (restaurant.Status != RestaurantStatus.Inactive)
            throw new InvalidOperationException($"Only inactive restaurants can be reactivated. Current status: {restaurant.Status}");

        // Get admin user info from HTTP context
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           "Unknown Admin";

        restaurant.Status = RestaurantStatus.Active;
        restaurant.UpdatedAt = DateTime.UtcNow;
        restaurant.RejectionReason = null; // Clear reason on reactivation

        await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

        // Publish restaurant approved/activated event
        await _publishEndpoint.Publish(new RestaurantApprovedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = restaurant.Id,
            Name = restaurant.Name,
            ApprovedBy = adminUserName
        }, cancellationToken);

        // Log audit trail
        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("Restaurant", id, "Reactivated", 
                "Restaurant restored to active status", adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
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

        // Publish deletion event to notify other services
        var deletedEvent = new RestaurantDeletedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventVersion = 1,
            RestaurantId = restaurantId,
            Name = restaurant.Name,
            DeletedBy = "Admin"
        };
        await _publishEndpoint.Publish(deletedEvent, cancellationToken);
    }
}
