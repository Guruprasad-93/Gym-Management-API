using System.Security.Cryptography;
using System.Text;
using Gym.Application.DTOs.Auth;
using Gym.Application.Interfaces;
using Gym.Application.Models;
using Gym.Application.Options;
using Gym.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionResolver _permissionResolver;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordResetSettings _passwordResetSettings;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ISaasSubscriptionRepository _saasRepository;
    private readonly IGymMenuRepository _gymMenuRepository;

    public AuthService(
        IUserRepository userRepository,
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IPermissionResolver permissionResolver,
        IJwtTokenGenerator jwtTokenGenerator,
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IOptions<JwtSettings> jwtSettings,
        IOptions<PasswordResetSettings> passwordResetSettings,
        IHostEnvironment hostEnvironment,
        ISaasSubscriptionRepository saasRepository,
        IGymMenuRepository gymMenuRepository)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _permissionResolver = permissionResolver;
        _jwtTokenGenerator = jwtTokenGenerator;
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _jwtSettings = jwtSettings.Value;
        _passwordResetSettings = passwordResetSettings.Value;
        _hostEnvironment = hostEnvironment;
        _saasRepository = saasRepository;
        _gymMenuRepository = gymMenuRepository;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto dto,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var loginIdentifier = Gym.Application.Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Gym.Application.Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var loginUser = await _authRepository.LoginUserAsync(loginIdentifier, dto.GymId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid login identifier or password.");

        if (!loginUser.UserIsActive)
            throw new UnauthorizedAccessException("Your account is inactive.");

        if (!_passwordHasher.Verify(dto.Password, loginUser.PasswordHash))
            throw new UnauthorizedAccessException("Invalid login identifier or password.");

        if (loginUser.GymId.HasValue && !loginUser.GymIsActive)
            throw new UnauthorizedAccessException("Your gym is inactive.");

        if (loginUser.GymId.HasValue)
            await EnsureSubscriptionAccessAsync(loginUser.GymId.Value, cancellationToken);

        await _authRepository.EndAllSessionsForUserAsync(loginUser.UserId, cancellationToken);
        await _authRepository.RevokeAllRefreshTokensForUserAsync(loginUser.UserId, cancellationToken);
        var tokenVersion = await _authRepository.IncrementTokenVersionAsync(loginUser.UserId, cancellationToken);

        var sessionId = Guid.NewGuid();
        await _authRepository.CreateSessionAsync(loginUser.UserId, sessionId, deviceInfo, ipAddress, cancellationToken);

        return await IssueTokensAsync(loginUser.UserId, tokenVersion, sessionId, deviceInfo, ipAddress, cancellationToken);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(
        string refreshToken,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token is required.");

        var tokenHash = HashRefreshToken(refreshToken);
        var stored = await _authRepository.GetRefreshTokenAsync(tokenHash, cancellationToken);
        if (stored is null)
        {
            var revokedUserId = await _authRepository.GetRevokedRefreshTokenUserIdAsync(tokenHash, cancellationToken);
            if (revokedUserId.HasValue)
            {
                await _authRepository.RevokeAllRefreshTokensForUserAsync(revokedUserId.Value, cancellationToken);
                await _authRepository.IncrementTokenVersionAsync(revokedUserId.Value, cancellationToken);
            }

            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(stored.Value.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Your account is inactive.");

        await ValidateGymAccessAsync(user.Id, cancellationToken);

        var loginContext = await _authRepository.GetLoginContextAsync(user.Id, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        var newRefreshToken = GenerateSecureToken();
        await _authRepository.RevokeRefreshTokenAsync(
            tokenHash,
            HashRefreshToken(newRefreshToken),
            cancellationToken);

        var sessionId = Guid.NewGuid();
        await _authRepository.CreateSessionAsync(user.Id, sessionId, deviceInfo, ipAddress, cancellationToken);

        return await BuildTokenResponseAsync(
            user.Id,
            loginContext.TokenVersion,
            sessionId,
            loginContext,
            deviceInfo,
            ipAddress,
            newRefreshToken,
            cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _authRepository.EndSessionAsync(sessionId, cancellationToken);
        await _authRepository.RevokeAllRefreshTokensForUserAsync(userId, cancellationToken);
        await _authRepository.IncrementTokenVersionAsync(userId, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        await _authRepository.ChangePasswordAsync(userId, _passwordHasher.Hash(dto.NewPassword), cancellationToken);
        await _authRepository.EndAllSessionsForUserAsync(userId, cancellationToken);
        await _authRepository.RevokeAllRefreshTokensForUserAsync(userId, cancellationToken);
    }

    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var loginIdentifier = Gym.Application.Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Gym.Application.Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var user = await _userRepository.GetByLoginIdentifierAsync(loginIdentifier, dto.GymId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new ForgotPasswordResponseDto
            {
                Message = "If the account exists, a password reset link has been sent."
            };
        }

        var resetToken = GenerateSecureToken();
        var tokenHash = HashToken(resetToken);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.PasswordResetTokenExpiryMinutes);

        await _authRepository.SetPasswordResetTokenAsync(loginIdentifier, dto.GymId, tokenHash, expiresAt, cancellationToken);

        var includeDevReset = _hostEnvironment.IsDevelopment() && _jwtSettings.ReturnResetTokenInDevelopment;
        string? resetLink = null;
        if (includeDevReset)
        {
            var baseUrl = _passwordResetSettings.FrontendBaseUrl.TrimEnd('/');
            var gymQuery = dto.GymId.HasValue ? $"&gymId={dto.GymId.Value}" : string.Empty;
            resetLink = $"{baseUrl}/auth/reset-password?loginIdentifier={Uri.EscapeDataString(loginIdentifier)}&token={Uri.EscapeDataString(resetToken)}{gymQuery}";
        }

        return new ForgotPasswordResponseDto
        {
            Message = "If the account exists, a password reset link has been sent.",
            ResetToken = includeDevReset ? resetToken : null,
            ResetLink = resetLink
        };
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var loginIdentifier = Gym.Application.Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Gym.Application.Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var tokenHash = HashToken(dto.Token.Trim());

        var success = await _authRepository.ResetPasswordAsync(
            loginIdentifier,
            dto.GymId,
            tokenHash,
            _passwordHasher.Hash(dto.NewPassword),
            cancellationToken);

        if (!success)
            throw new UnauthorizedAccessException("Invalid or expired reset token.");

        var user = await _userRepository.GetByLoginIdentifierAsync(loginIdentifier, dto.GymId, cancellationToken);
        if (user is not null)
        {
            await _authRepository.EndAllSessionsForUserAsync(user.Id, cancellationToken);
            await _authRepository.RevokeAllRefreshTokensForUserAsync(user.Id, cancellationToken);
        }
    }

    public Task<bool> ValidateSessionAsync(
        Guid userId,
        Guid sessionId,
        int tokenVersion,
        CancellationToken cancellationToken = default) =>
        _authRepository.IsSessionActiveAsync(userId, sessionId, tokenVersion, cancellationToken);

    public async Task<SessionPermissionsDto> GetSessionPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var loginContext = await _authRepository.GetLoginContextAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!loginContext.UserIsActive)
            throw new UnauthorizedAccessException("Your account is inactive.");

        if (loginContext.GymId.HasValue && !loginContext.GymIsActive)
            throw new UnauthorizedAccessException("Your gym is inactive.");

        await EnsureProfileRolesAsync(userId, cancellationToken);

        var roles = await _permissionResolver.GetRolesForUserAsync(userId, cancellationToken);
        var permissions = await _permissionResolver.GetPermissionsForUserAsync(userId, cancellationToken);
        var enabledMenuCodes = await GetEnabledMenuCodesAsync(loginContext.GymId, cancellationToken);

        return new SessionPermissionsDto
        {
            UserId = userId,
            FullName = loginContext.FullName,
            Email = loginContext.Email,
            GymId = loginContext.GymId,
            GymName = loginContext.GymName,
            Roles = roles,
            Permissions = permissions,
            EnabledMenuCodes = enabledMenuCodes,
            RefreshedAt = DateTime.UtcNow
        };
    }

    private async Task ValidateGymAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var context = await _authRepository.GetLoginContextAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!context.UserIsActive)
            throw new UnauthorizedAccessException("Your account is inactive.");

        if (context.GymId.HasValue && !context.GymIsActive)
            throw new UnauthorizedAccessException("Your gym is inactive. Contact support.");

        if (context.GymId.HasValue)
            await EnsureSubscriptionAccessAsync(context.GymId.Value, cancellationToken);
    }

    private async Task EnsureSubscriptionAccessAsync(Guid gymId, CancellationToken cancellationToken)
    {
        var subscription = await _saasRepository.GetGymSubscriptionAsync(gymId, cancellationToken);
        if (subscription is not null && !subscription.HasAccess)
            throw new UnauthorizedAccessException("Your subscription has expired. Please renew to continue.");
    }

    private async Task<LoginResponseDto> IssueTokensAsync(
        Guid userId,
        int tokenVersion,
        Guid sessionId,
        string? deviceInfo,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var loginContext = await _authRepository.GetLoginContextAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        var refreshToken = GenerateSecureToken();
        return await BuildTokenResponseAsync(
            userId,
            tokenVersion,
            sessionId,
            loginContext,
            deviceInfo,
            ipAddress,
            refreshToken,
            cancellationToken);
    }

    private async Task<LoginResponseDto> BuildTokenResponseAsync(
        Guid userId,
        int tokenVersion,
        Guid sessionId,
        UserLoginContext loginContext,
        string? deviceInfo,
        string? ipAddress,
        string refreshTokenPlain,
        CancellationToken cancellationToken)
    {
        await EnsureProfileRolesAsync(userId, cancellationToken);

        var roles = await _permissionResolver.GetRolesForUserAsync(userId, cancellationToken);
        var permissions = await _permissionResolver.GetPermissionsForUserAsync(userId, cancellationToken);
        var enabledMenuCodes = await GetEnabledMenuCodesAsync(loginContext.GymId, cancellationToken);

        var tokenContext = new TokenGenerationContext
        {
            UserId = userId,
            FullName = loginContext.FullName,
            Email = loginContext.Email,
            GymId = loginContext.GymId,
            Roles = roles,
            Permissions = permissions,
            TokenVersion = tokenVersion,
            SessionId = sessionId
        };

        var accessToken = _jwtTokenGenerator.GenerateToken(tokenContext);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        await _authRepository.CreateRefreshTokenAsync(
            userId,
            HashRefreshToken(refreshTokenPlain),
            refreshExpires,
            deviceInfo,
            ipAddress,
            cancellationToken);

        return new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshTokenPlain,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            RefreshTokenExpiresAt = refreshExpires,
            UserId = userId,
            FullName = loginContext.FullName,
            Email = loginContext.Email,
            GymId = loginContext.GymId,
            GymName = loginContext.GymName,
            SessionId = sessionId,
            TokenVersion = tokenVersion,
            Roles = roles,
            Permissions = permissions,
            EnabledMenuCodes = enabledMenuCodes,
            MustChangePassword = loginContext.MustChangePassword
        };
    }

    private async Task<IReadOnlyList<string>> GetEnabledMenuCodesAsync(Guid? gymId, CancellationToken cancellationToken)
    {
        if (!gymId.HasValue)
            return Array.Empty<string>();

        return await _gymMenuRepository.GetEnabledMenuCodesAsync(gymId.Value, cancellationToken);
    }

    /// <summary>
    /// Assigns Member/Trainer roles when the user has a profile but no role row (e.g. demo members created before role seeding).
    /// </summary>
    private async Task EnsureProfileRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (await _memberRepository.GetByUserIdAsync(userId, cancellationToken) is not null)
        {
            var memberRole = await _roleRepository.GetByNameAsync("Member", cancellationToken);
            if (memberRole is not null &&
                await _userRoleRepository.GetAsync(userId, memberRole.Id, cancellationToken) is null)
            {
                await _userRoleRepository.AddAsync(UserRole.Create(userId, memberRole.Id), cancellationToken);
            }
        }

        if (await _trainerRepository.GetByUserIdAsync(userId, cancellationToken) is not null)
        {
            var trainerRole = await _roleRepository.GetByNameAsync("Trainer", cancellationToken);
            if (trainerRole is not null &&
                await _userRoleRepository.GetAsync(userId, trainerRole.Id, cancellationToken) is null)
            {
                await _userRoleRepository.AddAsync(UserRole.Create(userId, trainerRole.Id), cancellationToken);
            }
        }
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashRefreshToken(string token) => HashToken(token);

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
