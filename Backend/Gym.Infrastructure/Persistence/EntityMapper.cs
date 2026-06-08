using Gym.Domain.Entities;
using Gym.Infrastructure.Persistence.Models;

namespace Gym.Infrastructure.Persistence;

internal static class EntityMapper
{
    public static User ToUser(UserRow row) =>
        User.Hydrate(
            row.Id,
            row.Name,
            row.Email,
            row.Password,
            row.GymId,
            row.CreatedDate,
            row.IsActive,
            row.TokenVersion,
            row.PasswordResetToken,
            row.PasswordResetTokenExpiresAt);

    public static Role ToRole(RoleRow row) =>
        Role.Hydrate(row.RoleId, row.RoleName, row.Description, row.IsSystemRole, row.CreatedDate, row.CreatedAt, row.UpdatedAt);

    public static Privilege ToPrivilege(PrivilegeRow row) =>
        Privilege.Hydrate(row.PrivilegeId, row.PrivilegeName, row.Description, row.Category, row.CreatedDate, row.CreatedAt, row.UpdatedAt);

    public static RolePrivilege ToRolePrivilege(RolePrivilegeRow row) =>
        RolePrivilege.Hydrate(row.RolePrivilegeId, row.RoleId, row.PrivilegeId, row.CreatedAt, row.UpdatedAt);

    public static RolePrivilege ToRolePrivilegeWithDetails(RolePrivilegeRow row) =>
        RolePrivilege.HydrateWithDetails(
            row.RolePrivilegeId,
            row.RoleId,
            row.RoleName ?? string.Empty,
            row.PrivilegeId,
            row.PrivilegeName ?? string.Empty,
            row.Category ?? string.Empty,
            row.CreatedAt,
            row.UpdatedAt);

    public static UserRole ToUserRole(UserRoleRow row)
    {
        if (!string.IsNullOrEmpty(row.UserName))
        {
            return UserRole.HydrateWithDetails(
                row.UserRoleId,
                row.UserId,
                row.UserName ?? string.Empty,
                row.UserEmail ?? string.Empty,
                row.RoleId,
                row.RoleName ?? string.Empty,
                row.CreatedAt,
                row.UpdatedAt);
        }

        return UserRole.Hydrate(row.UserRoleId, row.UserId, row.RoleId, row.CreatedAt, row.UpdatedAt);
    }
}
