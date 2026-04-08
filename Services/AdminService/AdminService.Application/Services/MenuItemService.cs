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

        if (request.RestaurantId == Guid.Empty)
            throw new ArgumentException("Restaurant ID is required", nameof(request));

        ValidateName(request.Name);
        ValidateDescription(request.Description);
        ValidatePrice(request.Price, nameof(request.Price));
        ValidateCategoryId(request.CategoryId);

        var normalizedCurrency = (request.Currency ?? "USD").ToUpperInvariant();
        
        // Create menu item entity
        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid(),
            RestaurantId = request.RestaurantId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = normalizedCurrency,
            CategoryId = request.CategoryId,
            Status = MenuItemStatus.Inactive,
            ApprovalStatus = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
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

        if (menuItem.ApprovalStatus == ApprovalStatus.Rejected)
            throw new InvalidOperationException("Cannot update rejected menu item. Create a new one instead.");

        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != menuItem.Name)
        {
            ValidateName(request.Name);
            menuItem.Name = request.Name;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description != menuItem.Description)
        {
            ValidateDescription(request.Description);
            menuItem.Description = request.Description;
            hasChanges = true;
        }

        if (request.Price.HasValue && !string.IsNullOrWhiteSpace(request.Currency))
        {
            ValidatePrice(request.Price.Value, nameof(request.Price));
            var normalizedCurrency = (request.Currency ?? "USD").ToUpperInvariant();
            if (menuItem.Price != request.Price.Value || !string.Equals(menuItem.Currency, normalizedCurrency, StringComparison.Ordinal))
            {
                menuItem.Price = request.Price.Value;
                menuItem.Currency = normalizedCurrency;
                hasChanges = true;
            }
        }

        if (request.CategoryId != menuItem.CategoryId)
        {
            ValidateCategoryId(request.CategoryId);
            menuItem.CategoryId = request.CategoryId;
            hasChanges = true;
        }

        if (hasChanges)
        {
            if (menuItem.ApprovalStatus == ApprovalStatus.Approved)
            {
                ResetApprovalToPending(menuItem);
            }

            menuItem.UpdatedAt = DateTime.UtcNow;
        }
        
        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        if (menuItem.ApprovalStatus != ApprovalStatus.Approved)
            throw new InvalidOperationException("Cannot activate menu item that is not approved");

        if (menuItem.Status != MenuItemStatus.Active)
        {
            menuItem.Status = MenuItemStatus.Active;
            menuItem.UpdatedAt = DateTime.UtcNow;
        }

        await _menuItemRepository.UpdateAsync(menuItem, cancellationToken);
        
        return _mapper.Map<MenuItemDto>(menuItem);
    }

    public async Task<MenuItemDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, cancellationToken);
        if (menuItem == null)
            throw new KeyNotFoundException($"Menu item with ID {id} not found");

        if (menuItem.Status != MenuItemStatus.Inactive)
        {
            menuItem.Status = MenuItemStatus.Inactive;
            menuItem.UpdatedAt = DateTime.UtcNow;
        }

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

        if (menuItem.ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve menu item with approval status: {menuItem.ApprovalStatus}");

        menuItem.ApprovalStatus = ApprovalStatus.Approved;
        menuItem.ApprovedBy = approvedBy;
        menuItem.ApprovedAt = DateTime.UtcNow;
        menuItem.ApprovalNotes = request.ApprovalNotes;
        menuItem.RejectionReason = null;
        menuItem.RejectedBy = null;
        menuItem.RejectedAt = null;
        menuItem.UpdatedAt = DateTime.UtcNow;

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

        if (menuItem.ApprovalStatus != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot reject menu item with approval status: {menuItem.ApprovalStatus}");

        menuItem.ApprovalStatus = ApprovalStatus.Rejected;
        menuItem.RejectedBy = rejectedBy;
        menuItem.RejectionReason = request.RejectionReason;
        menuItem.RejectedAt = DateTime.UtcNow;
        menuItem.Status = MenuItemStatus.Inactive;
        menuItem.ApprovalNotes = null;
        menuItem.ApprovedBy = null;
        menuItem.ApprovedAt = null;
        menuItem.UpdatedAt = DateTime.UtcNow;

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

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Menu item name is required", nameof(name));

        if (name.Length > 255)
            throw new ArgumentException("Menu item name cannot exceed 255 characters", nameof(name));

        if (name.Trim() != name)
            throw new ArgumentException("Menu item name cannot have leading or trailing whitespace", nameof(name));
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Menu item description is required", nameof(description));

        if (description.Length > 1000)
            throw new ArgumentException("Menu item description cannot exceed 1000 characters", nameof(description));
    }

    private static void ValidatePrice(decimal price, string parameterName)
    {
        if (price <= 0)
            throw new ArgumentException("Menu item price must be greater than zero", parameterName);

        if (price > 10000)
            throw new ArgumentException("Menu item price cannot exceed 10,000", parameterName);
    }

    private static void ValidateCategoryId(string? categoryId)
    {
        if (!string.IsNullOrWhiteSpace(categoryId) && categoryId.Length > 100)
            throw new ArgumentException("Category ID cannot exceed 100 characters", nameof(categoryId));
    }

    private static void ResetApprovalToPending(MenuItem menuItem)
    {
        menuItem.ApprovalStatus = ApprovalStatus.Pending;
        menuItem.ApprovedBy = null;
        menuItem.ApprovedAt = null;
        menuItem.ApprovalNotes = null;
    }
}