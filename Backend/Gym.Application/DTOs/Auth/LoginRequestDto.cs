using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [MaxLength(20)]
    public string LoginIdentifier { get; set; } = string.Empty;

    public Guid? GymId { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
