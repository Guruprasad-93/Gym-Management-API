using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Authorization;

public class AssignRolePrivilegeDto
{
    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PrivilegeId { get; set; }
}
