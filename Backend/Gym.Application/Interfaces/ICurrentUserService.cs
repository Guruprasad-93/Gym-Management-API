namespace Gym.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? GymId { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
    Guid RequireGymId();
}
