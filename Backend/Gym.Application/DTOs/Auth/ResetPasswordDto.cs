namespace Gym.Application.DTOs.Auth;

public class ResetPasswordDto
{
    public string LoginIdentifier { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
