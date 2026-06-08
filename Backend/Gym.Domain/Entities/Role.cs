namespace Gym.Domain.Entities;

public class Role : BaseEntity
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    public string RoleName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public ICollection<RolePrivilege> RolePrivileges { get; private set; } = new List<RolePrivilege>();
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    private Role() { }

    public static Role Create(string roleName, string? description, bool isSystemRole = false)
    {
        ValidateRoleName(roleName);

        return new Role
        {
            RoleName = roleName.Trim(),
            Description = description?.Trim(),
            IsSystemRole = isSystemRole,
            CreatedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string roleName, string? description)
    {
        ValidateRoleName(roleName);
        RoleName = roleName.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public static Role Hydrate(
        int id,
        string roleName,
        string? description,
        bool isSystemRole,
        DateTime createdDate,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            RoleName = roleName,
            Description = description,
            IsSystemRole = isSystemRole,
            CreatedDate = createdDate,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    private static void ValidateRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required.", nameof(roleName));

        if (roleName.Length > MaxNameLength)
            throw new ArgumentException($"Role name cannot exceed {MaxNameLength} characters.", nameof(roleName));
    }
}
