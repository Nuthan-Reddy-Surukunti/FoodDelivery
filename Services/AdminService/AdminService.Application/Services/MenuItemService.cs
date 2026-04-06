using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AdminService.Application.Services;

/// <summary>
/// Service implementation for menu item management with moderation capabilities
/// </summary>
public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MenuItemService(IMenuItemRepository menuItemRepository, IMapper mapper, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
    {
        _menuItemRepository = menuItemRepository ?? throw new ArgumentNullException(nameof(menuItemRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<MenuItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<PagedResultDto<MenuItemDto>> GetAllAsync(
        Guid? restaurantId = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? status = null,
        string? approvalStatus = null,
        CancellationToken cancellationToken = default)
    {
        MenuItemStatus? itemStatus = null;
        if (status != null && Enum.TryParse<MenuItemStatus>(status, true, out var parsedStatus))
        {
            itemStatus = parsedStatus;
        }

        ApprovalStatus? approvalStatusEnum = null;
        if (approvalStatus != null && Enum.TryParse<ApprovalStatus>(approvalStatus, true, out var parsedApprovalStatus))
        {
            approvalStatusEnum = parsedApprovalStatus;
        }

        var (items, totalCount) = await _menuItemRepository.GetPagedAsync(
            restaurantId, pageNumber, pageSize, itemStatus, approvalStatusEnum, cancellationToken);

        return new PagedResultDto<MenuItemDto>
        {
            Items = _mapper.Map<IEnumerable<MenuItemDto>>(items),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<MenuItemDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _menuItemRepository.GetPendingApprovalsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }

    public async Task<MenuItemDto> CreateAsync(CreateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Menu item name is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Menu item description is required", nameof(request));

        if (request.Price <= 0)
            throw new ArgumentException("Menu item price must be greater than zero", nameof(request));

        if (request.RestaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID is required", nameof(request));

        // Create Money value object
        var price = Money.Create(request.Price, request.Currency);
        
        // Create menu item entity
        var menuItem = MenuItem.Create(request.RestaurantId, request.Name, request.Description, price, request.CategoryId);
        
        // Save to repository
        await _menuItemRepository.AddAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        // Update price if provided
        Money? newPrice = null;
        if (request.Price.HasValue && !string.IsNullOrWhiteSpace(request.Currency))
        {
            if (request.Price.Value <= 0)
                throw new ArgumentException("Menu item price must be greater than zero", nameof(request.Price));
            
            newPrice = Money.Create(request.Price.Value, request.Currency);
        }

        // Update menu item details
        menuItem.UpdateDetails(request.Name, request.Description, newPrice, request.CategoryId);
        
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        menuItem.Activate();
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        menuItem.Deactivate();
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> ApproveAsync(Guid id, ApproveMenuItemRequest request, string approvedBy, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approved by is required", nameof(approvedBy));

        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        menuItem.Approve(approvedBy, request.ApprovalNotes);
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);

        // Log audit trail
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           approvedBy;

        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("MenuItem", id, "Approved", 
                request.ApprovalNotes, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> RejectAsync(Guid id, RejectMenuItemRequest request, string rejectedBy, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejected by is required", nameof(rejectedBy));

        if (string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(request.RejectionReason));

        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        menuItem.Reject(rejectedBy, request.RejectionReason);
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);

        // Log audit trail
        var httpContext = _httpContextAccessor.HttpContext;
        var adminUserIdClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              httpContext?.User?.FindFirst("sub")?.Value;
        var adminUserName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
                           httpContext?.User?.FindFirst("name")?.Value ??
                           httpContext?.User?.Identity?.Name ??
                           rejectedBy;

        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            await _auditService.LogApprovalActionAsync("MenuItem", id, "Rejected", 
                request.RejectionReason, adminUserId, adminUserName, ipAddress, userAgent, cancellationToken);
        }
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<IEnumerable<MenuItemDto>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        if (restaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID is required", nameof(restaurantId));

        var items = await _menuItemRepository.GetByRestaurantIdAsync(restaurantId, cancellationToken);
        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }

    public async Task<IEnumerable<MenuItemDto>> GetActiveByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        if (restaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID is required", nameof(restaurantId));

        var items = await _menuItemRepository.GetActiveByRestaurantIdAsync(restaurantId, cancellationToken);
        return _mapper.Map<IEnumerable<MenuItemDto>>(items);
    }
}