using Gym.Application.DTOs.Auth;

namespace Gym.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string? deviceInfo, string? ipAddress, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string? deviceInfo, string? ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<bool> ValidateSessionAsync(Guid userId, Guid sessionId, int tokenVersion, CancellationToken cancellationToken = default);
    Task<SessionPermissionsDto> GetSessionPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
