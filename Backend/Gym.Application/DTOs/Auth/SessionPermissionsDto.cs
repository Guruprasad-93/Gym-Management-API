namespace Gym.Application.DTOs.Auth;

public class SessionPermissionsDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public string? GymName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> EnabledMenuCodes { get; set; } = Array.Empty<string>();
    public DateTime RefreshedAt { get; set; }
}
