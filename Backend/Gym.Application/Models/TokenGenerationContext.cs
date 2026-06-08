namespace Gym.Application.Models;

public class TokenGenerationContext
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid? GymId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
    public int TokenVersion { get; init; }
    public Guid SessionId { get; init; }
}
