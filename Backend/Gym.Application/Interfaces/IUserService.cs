using Gym.Application.DTOs.Users;

namespace Gym.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
}
