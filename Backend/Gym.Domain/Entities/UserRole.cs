namespace Gym.Domain.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public int RoleId { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public static UserRole Create(Guid userId, int roleId) =>
        new()
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };

    public static UserRole Hydrate(
        int id,
        Guid userId,
        int roleId,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            UserId = userId,
            RoleId = roleId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public static UserRole HydrateWithDetails(
        int id,
        Guid userId,
        string userName,
        string userEmail,
        int roleId,
        string roleName,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            UserId = userId,
            RoleId = roleId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            User = User.Hydrate(userId, userName, userEmail, string.Empty, null, createdAt),
            Role = Role.Hydrate(roleId, roleName, null, false, createdAt, createdAt, null)
        };
}
