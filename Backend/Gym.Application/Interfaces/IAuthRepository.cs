namespace Gym.Application.Interfaces;

public interface IAuthRepository
{
    Task<Models.LoginUserResult?> LoginUserAsync(string loginIdentifier, Guid? gymId, CancellationToken cancellationToken = default);

    Task<Models.UserLoginContext?> GetLoginContextAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> ChangePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
    Task SetPasswordResetTokenAsync(string loginIdentifier, Guid? gymId, string resetToken, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string loginIdentifier, Guid? gymId, string resetToken, string passwordHash, CancellationToken cancellationToken = default);
    Task<int> IncrementTokenVersionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task EndAllSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> CreateSessionAsync(Guid userId, Guid sessionGuid, string? deviceInfo, string? ipAddress, CancellationToken cancellationToken = default);
    Task EndSessionAsync(Guid sessionGuid, CancellationToken cancellationToken = default);
    Task<bool> IsSessionActiveAsync(Guid userId, Guid sessionGuid, int tokenVersion, CancellationToken cancellationToken = default);
    Task<string> CreateRefreshTokenAsync(Guid userId, string token, DateTime expiresAt, string? deviceInfo, string? ipAddress, CancellationToken cancellationToken = default);
    Task<(Guid UserId, DateTime ExpiresAt)?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string? replacedBy = null, CancellationToken cancellationToken = default);
    Task RevokeAllRefreshTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid?> GetRevokedRefreshTokenUserIdAsync(string tokenHash, CancellationToken cancellationToken = default);
}
