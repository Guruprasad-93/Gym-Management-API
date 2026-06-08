using System.Security.Claims;



using Gym.Application.Constants;

using Gym.Application.DTOs.Auth;

using Gym.Application.DTOs.Common;

using Gym.Application.DTOs.Users;

using Gym.Application.Interfaces;



using Gym.Domain.Constants;



using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Gym.API.Extensions;







namespace Gym.API.Controllers;







/// <summary>Authentication and password management. Registration requires Super Admin or Gym Admin.</summary>



[ApiController]



[Route("api/[controller]")]



public class AuthController : ControllerBase



{



    private readonly IAuthService _authService;



    private readonly IUserService _userService;



    private readonly ICurrentUserService _currentUser;

    private readonly IAuditService _auditService;

    private readonly IAuthCookieService _authCookies;

    private readonly IWebHostEnvironment _environment;



    public AuthController(

        IAuthService authService,

        IUserService userService,

        ICurrentUserService currentUser,

        IAuditService auditService,

        IAuthCookieService authCookies,

        IWebHostEnvironment environment)

    {

        _authService = authService;

        _userService = userService;

        _currentUser = currentUser;

        _auditService = auditService;

        _authCookies = authCookies;

        _environment = environment;

    }







    /// <summary>Anonymous login. Sets httpOnly cookies when AuthCookies:UseCookieAuth is enabled.</summary>



    [HttpPost("login")]



    [AllowAnonymous]

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]



    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]



    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(



        [FromBody] LoginRequestDto dto,



        CancellationToken cancellationToken)



    {



        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _authService.LoginAsync(

            dto,

            Request.Headers.UserAgent.ToString(),

            ip,

            cancellationToken);



        await _auditService.LogAuthAsync(

            AuditActionTypes.Login,

            result.UserId,

            result.GymId,

            ip,

            cancellationToken);



        _authCookies.SetAuthCookies(Response, result, _environment.IsProduction());

        return Ok(ApiResponse<LoginResponseDto>.Ok(StripTokensIfCookieAuth(result), "Login successful."));



    }







    /// <summary>Issues a CSRF token cookie for double-submit validation.</summary>



    [HttpGet("csrf")]



    [AllowAnonymous]



    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]



    public ActionResult<ApiResponse<object>> GetCsrfToken()



    {



        var token = _authCookies.IssueCsrfToken(Response, _environment.IsProduction());

        return Ok(ApiResponse<object>.Ok(new { token }, "CSRF token issued."));



    }







    /// <summary>Validates the current JWT and returns claim summary.</summary>



    [Authorize]



    [HttpGet("validate")]



    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]



    [ProducesResponseType(StatusCodes.Status401Unauthorized)]



    public ActionResult<ApiResponse<object>> ValidateToken()



    {



        return Ok(ApiResponse<object>.Ok(new



        {



            userId = User.FindFirstValue(AuthClaimTypes.UserId),



            fullName = User.FindFirstValue(AuthClaimTypes.FullName),



            email = User.FindFirstValue(AuthClaimTypes.Email),



            gymId = User.FindFirstValue(AuthClaimTypes.GymId),



            roles = User.FindAll(AuthClaimTypes.Role).Select(c => c.Value).ToList(),



            permissions = User.FindAll(AuthClaimTypes.Permission).Select(c => c.Value).ToList(),



            tokenVersion = User.FindFirstValue(AuthClaimTypes.TokenVersion),



            sessionId = User.FindFirstValue(AuthClaimTypes.SessionId)



        }, "Token is valid."));



    }







    /// <summary>Reloads roles and permissions for the current session without re-login.</summary>

    [Authorize]

    [HttpGet("session")]

    [ProducesResponseType(typeof(ApiResponse<SessionPermissionsDto>), StatusCodes.Status200OK)]

    public async Task<ActionResult<ApiResponse<SessionPermissionsDto>>> GetSession(

        CancellationToken cancellationToken)

    {

        var permissions = await _authService.GetSessionPermissionsAsync(ParseUserId(), cancellationToken);

        return Ok(ApiResponse<SessionPermissionsDto>.Ok(permissions, "Session permissions refreshed."));

    }



    /// <summary>Ends the current session and revokes refresh tokens.</summary>



    [Authorize]



    [HttpPost("logout")]



    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]



    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)



    {



        var userId = ParseUserId();

        var sessionId = ParseSessionId();

        var gymId = Guid.TryParse(User.FindFirstValue(AuthClaimTypes.GymId), out var gid) ? gid : (Guid?)null;

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();



        await _authService.LogoutAsync(userId, sessionId, cancellationToken);

        _authCookies.ClearAuthCookies(Response);



        await _auditService.LogAuthAsync(AuditActionTypes.Logout, userId, gymId, ip, cancellationToken);



        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));



    }







    /// <summary>Exchanges a refresh token for a new access token.</summary>



    [HttpPost("refresh")]



    [AllowAnonymous]

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]



    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]



    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Refresh(



        [FromBody] RefreshTokenRequestDto? dto,



        CancellationToken cancellationToken)



    {



        var refreshToken = _authCookies.GetRefreshToken(Request)

            ?? dto?.RefreshToken

            ?? string.Empty;



        var result = await _authService.RefreshTokenAsync(



            refreshToken,



            Request.Headers.UserAgent.ToString(),



            HttpContext.Connection.RemoteIpAddress?.ToString(),



            cancellationToken);



        _authCookies.SetAuthCookies(Response, result, _environment.IsProduction());



        return Ok(ApiResponse<LoginResponseDto>.Ok(StripTokensIfCookieAuth(result), "Token refreshed successfully."));



    }







    /// <summary>Changes password for the authenticated user. Invalidates other sessions.</summary>



    [Authorize]



    [HttpPost("change-password")]



    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]



    [ProducesResponseType(StatusCodes.Status401Unauthorized)]



    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(



        [FromBody] ChangePasswordDto dto,



        CancellationToken cancellationToken)



    {



        await _authService.ChangePasswordAsync(ParseUserId(), dto, cancellationToken);

        _authCookies.ClearAuthCookies(Response);



        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully. Please sign in again."));



    }







    /// <summary>Initiates password reset (always returns generic success message).</summary>



    [HttpPost("forgot-password")]



    [AllowAnonymous]

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]



    [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponseDto>), StatusCodes.Status200OK)]



    public async Task<ActionResult<ApiResponse<ForgotPasswordResponseDto>>> ForgotPassword(



        [FromBody] ForgotPasswordDto dto,



        CancellationToken cancellationToken)



    {



        var result = await _authService.ForgotPasswordAsync(dto, cancellationToken);



        return Ok(ApiResponse<ForgotPasswordResponseDto>.Ok(result));



    }







    /// <summary>Completes password reset using email and token from forgot-password flow.</summary>



    [HttpPost("reset-password")]



    [AllowAnonymous]

    [EnableRateLimiting(RateLimitingExtensions.AuthPolicyName)]



    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]



    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(



        [FromBody] ResetPasswordDto dto,



        CancellationToken cancellationToken)



    {



        await _authService.ResetPasswordAsync(dto, cancellationToken);



        return Ok(ApiResponse<object>.Ok(null!, "Password reset successfully. You can sign in with your new password."));



    }







    /// <summary>Registers a user. Restricted to Super Admin and Gym Admin (Gym Admin scoped to own gym).</summary>



    [Authorize]



    [HttpPost("register")]



    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]



    [ProducesResponseType(StatusCodes.Status401Unauthorized)]



    [ProducesResponseType(StatusCodes.Status403Forbidden)]



    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Register(



        [FromBody] RegisterUserDto dto,



        CancellationToken cancellationToken)



    {



        if (!RoleNames.UserRegistrationAllowed.Any(_currentUser.HasRole))



            return StatusCode(StatusCodes.Status403Forbidden,



                ApiResponse<UserResponseDto>.Fail("Only Super Admin or Gym Admin can register users."));







        var user = await _userService.RegisterAsync(dto, cancellationToken);



        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserResponseDto>.Ok(user, "User registered successfully."));



    }







    private LoginResponseDto StripTokensIfCookieAuth(LoginResponseDto result)



    {



        if (!_authCookies.UseCookieAuth)



            return result;







        return new LoginResponseDto



        {



            Token = string.Empty,



            RefreshToken = string.Empty,



            ExpiresAt = result.ExpiresAt,



            RefreshTokenExpiresAt = result.RefreshTokenExpiresAt,



            UserId = result.UserId,



            FullName = result.FullName,



            Email = result.Email,



            GymId = result.GymId,



            GymName = result.GymName,



            SessionId = result.SessionId,



            TokenVersion = result.TokenVersion,



            Roles = result.Roles,



            Permissions = result.Permissions,



            MustChangePassword = result.MustChangePassword



        };



    }







    private Guid ParseUserId()



    {



        var claim = User.FindFirstValue(AuthClaimTypes.UserId);



        return Guid.TryParse(claim, out var id)



            ? id



            : throw new UnauthorizedAccessException("Invalid token.");



    }







    private Guid ParseSessionId()



    {



        var claim = User.FindFirstValue(AuthClaimTypes.SessionId);



        return Guid.TryParse(claim, out var id)



            ? id



            : throw new UnauthorizedAccessException("Invalid session.");



    }



}




