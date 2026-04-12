using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IAuthRepository
{
    Task<IEnumerable<User>> GetPendingApprovalRestaurantsAsync();
    Task<bool> ApproveRestaurantAsync(Guid restaurantId, Guid adminId, string? notes);
    Task<bool> RejectRestaurantAsync(Guid restaurantId, Guid adminId, string? notes);
}
