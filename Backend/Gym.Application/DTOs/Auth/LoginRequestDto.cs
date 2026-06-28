using System.ComponentModel.DataAnnotations;

namespace Gym.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [MaxLength(100)]
    public string LoginIdentifier { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
