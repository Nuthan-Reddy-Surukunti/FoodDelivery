using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IRestaurantApprovalService
{
    Task<IEnumerable<User>> GetPendingRestaurantsAsync();
    Task<bool> ApproveRestaurantAsync(Guid restaurantId, Guid adminId, string? notes);
    Task<bool> RejectRestaurantAsync(Guid restaurantId, Guid adminId, string? notes);
}
