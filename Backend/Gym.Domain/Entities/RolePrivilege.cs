namespace Gym.Domain.Entities;

public class RolePrivilege : BaseEntity
{
    public int RoleId { get; private set; }
    public int PrivilegeId { get; private set; }

    public Role Role { get; private set; } = null!;
    public Privilege Privilege { get; private set; } = null!;

    private RolePrivilege() { }

    public static RolePrivilege Create(int roleId, int privilegeId) =>
        new()
        {
            RoleId = roleId,
            PrivilegeId = privilegeId,
            CreatedAt = DateTime.UtcNow
        };

    public static RolePrivilege Hydrate(
        int id,
        int roleId,
        int privilegeId,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            RoleId = roleId,
            PrivilegeId = privilegeId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

    public static RolePrivilege HydrateWithDetails(
        int id,
        int roleId,
        string roleName,
        int privilegeId,
        string privilegeName,
        string category,
        DateTime createdAt,
        DateTime? updatedAt) =>
        new()
        {
            Id = id,
            RoleId = roleId,
            PrivilegeId = privilegeId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Role = Role.Hydrate(roleId, roleName, null, false, createdAt, createdAt, null),
            Privilege = Privilege.Hydrate(privilegeId, privilegeName, null, category, createdAt, createdAt, null)
        };
}
