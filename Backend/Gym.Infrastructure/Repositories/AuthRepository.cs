using System.Data;
using Dapper;
using Gym.Application.Interfaces;
using Gym.Application.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public AuthRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<LoginUserResult?> LoginUserAsync(string email, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<LoginUserRow>(
            StoredProcedureNames.LoginUser,
            new { Email = email.Trim().ToLowerInvariant() },
            cancellationToken);

        if (row is null) return null;

        return new LoginUserResult
        {
            UserId = row.UserId,
            FullName = row.FullName,
            Email = row.Email,
            PasswordHash = row.Password,
            GymId = row.GymId,
            UserIsActive = row.UserIsActive,
            TokenVersion = row.TokenVersion,
            MustChangePassword = row.MustChangePassword,
            GymName = row.GymName,
            GymIsActive = row.GymIsActive
        };
    }

    public async Task<UserLoginContext?> GetLoginContextAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<UserLoginContextRow>(
            StoredProcedureNames.GetUserLoginContext,
            new { UserId = userId },
            cancellationToken);

        if (row is null) return null;

        return new UserLoginContext
        {
            UserId = row.UserId,
            FullName = row.FullName,
            Email = row.Email,
            GymId = row.GymId,
            UserIsActive = row.UserIsActive,
            TokenVersion = row.TokenVersion,
            GymName = row.GymName,
            GymIsActive = row.GymIsActive,
            MustChangePassword = row.MustChangePassword
        };
    }

    public async Task<int> ChangePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default) =>
        await _sp.QuerySingleOrDefaultAsync<int>(
            StoredProcedureNames.ChangeUserPassword,
            new { UserId = userId, PasswordHash = passwordHash },
            cancellationToken);

    public Task SetPasswordResetTokenAsync(string email, string resetToken, DateTime expiresAt, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.SetPasswordResetToken,
            new { Email = email, ResetToken = resetToken, ExpiresAt = expiresAt },
            cancellationToken);

    public async Task<bool> ResetPasswordAsync(string email, string resetToken, string passwordHash, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QuerySingleOrDefaultAsync<int>(
            StoredProcedureNames.ResetUserPassword,
            new { Email = email, ResetToken = resetToken, PasswordHash = passwordHash },
            cancellationToken);

        return rows > 0;
    }

    public async Task<int> IncrementTokenVersionAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _sp.QuerySingleOrDefaultAsync<int>(
            StoredProcedureNames.IncrementTokenVersion,
            new { UserId = userId },
            cancellationToken);

    public Task EndAllSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.EndAllUserLoginSessions,
            new { UserId = userId },
            cancellationToken);

    public async Task<Guid> CreateSessionAsync(
        Guid userId,
        Guid sessionGuid,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@LoginSessionGuid", sessionGuid);
        parameters.Add("@DeviceInfo", deviceInfo);
        parameters.Add("@IpAddress", ipAddress);
        parameters.Add("@UserLoginSessionId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateUserLoginSession,
            parameters,
            "@UserLoginSessionId",
            cancellationToken);

        return sessionGuid;
    }

    public Task EndSessionAsync(Guid sessionGuid, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.EndUserLoginSession,
            new { LoginSessionGuid = sessionGuid },
            cancellationToken);

    public async Task<bool> IsSessionActiveAsync(
        Guid userId,
        Guid sessionGuid,
        int tokenVersion,
        CancellationToken cancellationToken = default) =>
        await _sp.QuerySingleOrDefaultAsync<bool>(
            StoredProcedureNames.IsUserLoginSessionActive,
            new { UserId = userId, LoginSessionGuid = sessionGuid, TokenVersion = tokenVersion },
            cancellationToken);

    public async Task<string> CreateRefreshTokenAsync(
        Guid userId,
        string token,
        DateTime expiresAt,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@Token", token);
        parameters.Add("@ExpiresAt", expiresAt);
        parameters.Add("@DeviceInfo", deviceInfo);
        parameters.Add("@IpAddress", ipAddress);
        parameters.Add("@RefreshTokenId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.InsertRefreshToken,
            parameters,
            "@RefreshTokenId",
            cancellationToken);

        return token;
    }

    public async Task<(Guid UserId, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<RefreshTokenRow>(
            StoredProcedureNames.GetRefreshTokenByToken,
            new { Token = token },
            cancellationToken);

        return row is null ? null : (row.UserId, row.ExpiresAt);
    }

    public Task RevokeRefreshTokenAsync(string token, string? replacedBy = null, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.RevokeRefreshToken,
            new { Token = token, ReplacedByToken = replacedBy },
            cancellationToken);

    public Task RevokeAllRefreshTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.RevokeAllRefreshTokensForUser,
            new { UserId = userId },
            cancellationToken);

    public async Task<Guid?> GetRevokedRefreshTokenUserIdAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var userId = await _sp.QuerySingleOrDefaultAsync<Guid?>(
            StoredProcedureNames.GetRevokedRefreshTokenByToken,
            new { Token = tokenHash },
            cancellationToken);

        return userId;
    }
}

internal sealed class LoginUserRow
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public bool UserIsActive { get; set; }
    public int TokenVersion { get; set; }
    public bool MustChangePassword { get; set; }
    public string? GymName { get; set; }
    public bool GymIsActive { get; set; }
}

internal sealed class UserLoginContextRow
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public bool UserIsActive { get; set; }
    public int TokenVersion { get; set; }
    public string? GymName { get; set; }
    public bool GymIsActive { get; set; }
    public bool MustChangePassword { get; set; }
}

internal sealed class RefreshTokenRow
{
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
