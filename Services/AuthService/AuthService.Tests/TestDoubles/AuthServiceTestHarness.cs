using System.Reflection;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthService.Tests.TestDoubles;

internal sealed class AuthServiceTestHarness
{
    public FakeUserRepository Users { get; } = new();
    public FakePasswordResetTokenRepository PasswordResetTokens { get; } = new();
    public FakeEmailVerificationTokenRepository EmailVerificationTokens { get; } = new();
    public FakeTwoFactorTokenRepository TwoFactorTokens { get; } = new();
    public FakeOtpTokenRepository OtpTokens { get; } = new();
    public FakeRefreshTokenRepository RefreshTokens { get; } = new();
    public FakeEmailService Email { get; } = new();
    public FakeJwtTokenGenerator Jwt { get; } = new();
    public FakeOtpService Otp { get; } = new();
    public RecordingPublishEndpoint Publisher { get; } = RecordingPublishEndpoint.Create();

    public Application.Services.AuthService CreateService()
    {
        return new Application.Services.AuthService(
            Users,
            PasswordResetTokens,
            EmailVerificationTokens,
            TwoFactorTokens,
            OtpTokens,
            RefreshTokens,
            Email,
            Jwt,
            Otp,
            Publisher.Endpoint,
            NullLogger<Application.Services.AuthService>.Instance);
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _usersById = new();
    private readonly Dictionary<Guid, string> _passwordsById = new();

    public bool CreateUserResult { get; set; } = true;
    public bool UpdatePasswordResult { get; set; } = true;
    public bool ChangePasswordResult { get; set; } = true;
    public bool UpdateUserResult { get; set; } = true;
    public bool DeleteUserResult { get; set; } = true;
    public bool SetTwoFactorEnabledResult { get; set; } = true;
    public bool SetEmailVerifiedResult { get; set; } = true;

    public IReadOnlyCollection<User> Users => _usersById.Values.ToList();

    public void Add(User user, string password = "Password123!")
    {
        _usersById[user.Id] = user;
        _passwordsById[user.Id] = password;
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        return Task.FromResult(_usersById.Values.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<User?> FindByIdAsync(Guid UserId)
    {
        _usersById.TryGetValue(UserId, out var user);
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_usersById.Values.ToList());
    }

    public Task<bool> CreateUserAsync(User user, string password)
    {
        if (!CreateUserResult)
            return Task.FromResult(false);

        if (user.Id == Guid.Empty)
            user.Id = Guid.NewGuid();

        Add(user, password);
        return Task.FromResult(true);
    }

    public Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        return Task.FromResult(_passwordsById.TryGetValue(userId, out var stored) && stored == password);
    }

    public Task<bool> SetEmailVerifiedAsync(Guid userId)
    {
        if (SetEmailVerifiedResult && _usersById.TryGetValue(userId, out var user))
            user.IsEmailVerified = true;

        return Task.FromResult(SetEmailVerifiedResult);
    }

    public Task<bool> SetAccountStatusAsync(Guid userId, AccountStatus accountStatus)
    {
        if (_usersById.TryGetValue(userId, out var user))
            user.AccountStatus = accountStatus;

        return Task.FromResult(true);
    }

    public Task<bool> SetTwoFactorEnabledAsync(Guid userId, bool enabled)
    {
        if (SetTwoFactorEnabledResult && _usersById.TryGetValue(userId, out var user))
            user.IsTwoFactorVerified = enabled;

        return Task.FromResult(SetTwoFactorEnabledResult);
    }

    public Task<bool> UpdatePasswordAsync(Guid userId, string newPassword)
    {
        if (UpdatePasswordResult && _usersById.ContainsKey(userId))
            _passwordsById[userId] = newPassword;

        return Task.FromResult(UpdatePasswordResult);
    }

    public Task<bool> ChangePasswordAsync(Guid userId, string newPassword)
    {
        if (ChangePasswordResult && _usersById.ContainsKey(userId))
            _passwordsById[userId] = newPassword;

        return Task.FromResult(ChangePasswordResult);
    }

    public Task<bool> UpdateUserAsync(User user)
    {
        if (UpdateUserResult)
            _usersById[user.Id] = user;

        return Task.FromResult(UpdateUserResult);
    }

    public Task<bool> DeleteUserAsync(Guid userId)
    {
        if (DeleteUserResult)
        {
            _usersById.Remove(userId);
            _passwordsById.Remove(userId);
        }

        return Task.FromResult(DeleteUserResult);
    }

    public Task<bool> IsAdminAsync(Guid userId) => Task.FromResult(GetRole(userId) == UserRole.Admin);
    public Task<bool> IsUserAsync(Guid userId) => Task.FromResult(_usersById.ContainsKey(userId));
    public Task<bool> IsRestaurantAsync(Guid userId) => Task.FromResult(GetRole(userId) == UserRole.RestaurantPartner);
    public Task<UserRole?> GetUserRoleAsync(Guid userId) => Task.FromResult(GetRole(userId));

    private UserRole? GetRole(Guid userId)
    {
        return _usersById.TryGetValue(userId, out var user) ? user.Role : null;
    }
}

internal sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly Dictionary<string, PasswordResetToken> _tokens = new();
    public List<Guid> MarkedUsedIds { get; } = new();

    public void AddExisting(PasswordResetToken token) => _tokens[token.Token] = token;

    public Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        _tokens.TryGetValue(token, out var resetToken);
        return Task.FromResult(resetToken);
    }

    public Task AddAsync(PasswordResetToken token)
    {
        _tokens[token.Token] = token;
        return Task.CompletedTask;
    }

    public Task MarkUsedAsync(Guid tokenId)
    {
        MarkedUsedIds.Add(tokenId);
        var token = _tokens.Values.FirstOrDefault(t => t.Id == tokenId);
        if (token != null)
            token.IsUsed = true;

        return Task.CompletedTask;
    }
}

internal sealed class FakeEmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly List<EmailVerificationToken> _tokens = new();
    public List<Guid> MarkedUsedIds { get; } = new();
    public IReadOnlyList<EmailVerificationToken> Tokens => _tokens;

    public Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId)
    {
        return Task.FromResult(_tokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault());
    }

    public Task AddAsync(EmailVerificationToken token)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task MarkUsedAsync(Guid tokenId)
    {
        MarkedUsedIds.Add(tokenId);
        var token = _tokens.FirstOrDefault(t => t.Id == tokenId);
        if (token != null)
            token.IsUsed = true;

        return Task.CompletedTask;
    }
}

internal sealed class FakeTwoFactorTokenRepository : ITwoFactorTokenRepository
{
    private readonly List<TwoFactorToken> _tokens = new();
    public List<Guid> MarkedUsedIds { get; } = new();
    public IReadOnlyList<TwoFactorToken> Tokens => _tokens;

    public Task<TwoFactorToken?> GetByTempTokenAsync(string tempToken)
    {
        return Task.FromResult(_tokens.FirstOrDefault(t => t.TempToken == tempToken));
    }

    public Task AddAsync(TwoFactorToken token)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task MarkUsedAsync(Guid tokenId)
    {
        MarkedUsedIds.Add(tokenId);
        var token = _tokens.FirstOrDefault(t => t.Id == tokenId);
        if (token != null)
            token.IsUsed = true;

        return Task.CompletedTask;
    }
}

internal sealed class FakeOtpTokenRepository : IOtpTokenRepository
{
    private readonly List<OtpToken> _tokens = new();
    public IReadOnlyList<OtpToken> Tokens => _tokens;

    public Task<OtpToken?> GetByUserIdAsync(Guid userId)
    {
        return Task.FromResult(_tokens.FirstOrDefault(t => t.UserId == userId));
    }

    public Task<OtpToken?> GetByOtpCodeAsync(Guid userId, string otpCode)
    {
        return Task.FromResult(_tokens.FirstOrDefault(t => t.UserId == userId && t.OTP == otpCode));
    }

    public Task AddAsync(OtpToken otpToken)
    {
        _tokens.Add(otpToken);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(OtpToken otpToken)
    {
        var index = _tokens.FindIndex(t => t.Id == otpToken.Id);
        if (index >= 0)
            _tokens[index] = otpToken;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid otpTokenId)
    {
        _tokens.RemoveAll(t => t.Id == otpTokenId);
        return Task.CompletedTask;
    }
}

internal sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly Dictionary<string, RefreshToken> _tokens = new();
    public List<RefreshToken> AddedTokens { get; } = new();
    public List<string> RevokedTokens { get; } = new();

    public void AddExisting(RefreshToken token)
    {
        _tokens[token.Token] = token;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token)
    {
        _tokens.TryGetValue(token, out var refreshToken);
        return Task.FromResult(refreshToken);
    }

    public Task AddAsync(RefreshToken token)
    {
        AddedTokens.Add(token);
        _tokens[token.Token] = token;
        return Task.CompletedTask;
    }

    public Task RevokeAsync(string token)
    {
        RevokedTokens.Add(token);
        if (_tokens.TryGetValue(token, out var refreshToken))
            refreshToken.IsRevoked = true;

        return Task.CompletedTask;
    }
}

internal sealed class FakeEmailService : IEmailService
{
    public List<SentEmail> SentEmails { get; } = new();

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        SentEmails.Add(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }
}

internal sealed record SentEmail(string ToEmail, string Subject, string Body);

internal sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string TokenToReturn { get; set; } = "jwt-token";
    public List<User> Users { get; } = new();

    public string GenerateToken(User user)
    {
        Users.Add(user);
        return TokenToReturn;
    }
}

internal sealed class FakeOtpService : IOtpService
{
    public bool GenerateAndStoreOtpResult { get; set; } = true;
    public bool VerifyOtpResult { get; set; } = true;
    public List<Guid> GeneratedForUserIds { get; } = new();
    public List<(Guid UserId, string Otp)> VerifiedOtps { get; } = new();

    public Task<bool> GenerateAndStoreOtpAsync(Guid userId)
    {
        GeneratedForUserIds.Add(userId);
        return Task.FromResult(GenerateAndStoreOtpResult);
    }

    public Task<bool> VerifyOtpAsync(Guid userId, string otpCode)
    {
        VerifiedOtps.Add((userId, otpCode));
        return Task.FromResult(VerifyOtpResult);
    }
}

internal class RecordingPublishEndpoint : DispatchProxy
{
    public IPublishEndpoint Endpoint { get; private set; } = null!;
    public List<object> PublishedMessages { get; } = new();

    public static RecordingPublishEndpoint Create()
    {
        var endpoint = DispatchProxy.Create<IPublishEndpoint, RecordingPublishEndpoint>();
        var proxy = (RecordingPublishEndpoint)(object)endpoint;
        proxy.Endpoint = endpoint;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod?.Name == nameof(IPublishEndpoint.Publish) && args?.Length > 0 && args[0] is object message)
            PublishedMessages.Add(message);

        if (targetMethod?.ReturnType == typeof(Task))
            return Task.CompletedTask;

        return targetMethod?.ReturnType.IsValueType == true
            ? Activator.CreateInstance(targetMethod.ReturnType)
            : null;
    }
}
