namespace Gym.Application.DTOs.GymAdmins;

public class ResendTemporaryPasswordResultDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
