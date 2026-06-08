namespace Gym.Application.DTOs.Auth;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    /// <summary>Development-only reset token (never returned in Production).</summary>
    public string? ResetToken { get; set; }
    /// <summary>Development-only link to Angular reset page with query parameters.</summary>
    public string? ResetLink { get; set; }
}
