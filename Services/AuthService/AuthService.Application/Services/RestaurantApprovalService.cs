using System;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MassTransit;
using QuickBite.Shared.Events.Auth;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services;

public class RestaurantApprovalService : IRestaurantApprovalService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RestaurantApprovalService> _logger;

    public RestaurantApprovalService(
        IAuthRepository authRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<RestaurantApprovalService> logger)
    {
        _authRepository = authRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetPendingRestaurantsAsync()
    {
        _logger.LogInformation("Fetching pending restaurant approvals");
        return await _authRepository.GetPendingApprovalRestaurantsAsync();
    }

    public async Task<bool> ApproveRestaurantAsync(Guid restaurantId, Guid adminId, string? notes)
    {
        try
        {
            _logger.LogInformation("Approving restaurant {RestaurantId} by admin {AdminId}", restaurantId, adminId);

            var result = await _authRepository.ApproveRestaurantAsync(restaurantId, adminId, notes);

            if (result)
            {
                await _publishEndpoint.Publish(new RestaurantApprovedEvent
                {
                    RestaurantId = restaurantId,
                    ApprovedByAdminId = adminId,
                    ApprovedAt = DateTime.UtcNow,
                    ApprovalNotes = notes
                });

                _logger.LogInformation("Restaurant {RestaurantId} approved successfully", restaurantId);
            }
            else
            {
                _logger.LogWarning("Failed to approve restaurant {RestaurantId}", restaurantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving restaurant {RestaurantId}", restaurantId);
            throw;
        }
    }

    public async Task<bool> RejectRestaurantAsync(Guid restaurantId, Guid adminId, string? notes)
    {
        try
        {
            _logger.LogInformation("Rejecting restaurant {RestaurantId} by admin {AdminId}", restaurantId, adminId);

            var result = await _authRepository.RejectRestaurantAsync(restaurantId, adminId, notes);

            if (result)
            {
                await _publishEndpoint.Publish(new RestaurantRejectedEvent
                {
                    RestaurantId = restaurantId,
                    RejectedByAdminId = adminId,
                    RejectedAt = DateTime.UtcNow,
                    RejectionReason = notes
                });

                _logger.LogInformation("Restaurant {RestaurantId} rejected successfully", restaurantId);
            }
            else
            {
                _logger.LogWarning("Failed to reject restaurant {RestaurantId}", restaurantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting restaurant {RestaurantId}", restaurantId);
            throw;
        }
    }
}
