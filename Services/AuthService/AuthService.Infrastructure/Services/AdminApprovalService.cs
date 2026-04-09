using AuthService.Application.Interfaces;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Identity;
using FoodDelivery.Shared.Events.Auth;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Services;

public class AdminApprovalService : IAdminApprovalService
{
    private readonly AuthDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AdminApprovalService(
        AuthDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<dynamic>> GetPendingApprovalsAsync()
    {
        return await _dbContext.Users
            .Where(u => u.AccountStatus == 0) // Pending status
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> ApproveUserAsync(Guid userId, Guid approvingAdminId, string? notes = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            // Only approve pending users
            if (user.AccountStatus != 0) // Not pending
                return false;

            // Set to Active status (still needs OTP verification)
            user.AccountStatus = 1;
            user.ApprovedByAdminId = approvingAdminId.ToString();
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovalNotes = notes;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Your Account Has Been Approved",
                    $"Congratulations! Your {user.Role} account has been approved. On your first login, a verification OTP will be sent to your email. Verify that OTP to complete activation and log in.");

                // Publish UserApprovedEvent to notify other services
                await _publishEndpoint.Publish(new UserApprovedEvent
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = DateTime.UtcNow,
                    EventVersion = 1,
                    UserId = Guid.Parse(user.Id),
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                });

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RejectUserAsync(Guid userId, Guid rejectingAdminId, string reason)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            // Only reject pending users
            if (user.AccountStatus != 0) // Not pending
                return false;

            // Set to Rejected status
            user.AccountStatus = 3;
            user.ApprovedByAdminId = rejectingAdminId.ToString();
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovalNotes = reason;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Your Account Application Status",
                    $"Your {user.Role} account application has been rejected. Reason: {reason}");

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
