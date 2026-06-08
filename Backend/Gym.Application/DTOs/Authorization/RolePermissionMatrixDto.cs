namespace Gym.Application.DTOs.Authorization;

public class RolePermissionMatrixDto
{
    public IReadOnlyList<RoleMatrixColumnDto> Roles { get; set; } = Array.Empty<RoleMatrixColumnDto>();
    public IReadOnlyList<PrivilegeMatrixRowDto> Privileges { get; set; } = Array.Empty<PrivilegeMatrixRowDto>();
    public IReadOnlyList<MatrixAssignmentDto> Assignments { get; set; } = Array.Empty<MatrixAssignmentDto>();
}

public class RoleMatrixColumnDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class PrivilegeMatrixRowDto
{
    public int PrivilegeId { get; set; }
    public string PrivilegeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class MatrixAssignmentDto
{
    public int RoleId { get; set; }
    public int PrivilegeId { get; set; }
    public bool Assigned { get; set; }
}
