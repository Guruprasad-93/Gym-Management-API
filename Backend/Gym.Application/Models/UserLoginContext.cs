namespace Gym.Application.Models;

public class UserLoginContext
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid? GymId { get; init; }
    public bool UserIsActive { get; init; }
    public int TokenVersion { get; init; }
    public string? GymName { get; init; }
    public bool GymIsActive { get; init; }
    public bool MustChangePassword { get; init; }
}
