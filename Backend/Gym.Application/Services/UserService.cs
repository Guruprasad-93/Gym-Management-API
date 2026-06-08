using Gym.Application.DTOs.Users;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<UserResponseDto> RegisterAsync(
        RegisterUserDto dto,
        CancellationToken cancellationToken = default)
    {
        EnsureCanRegisterUsers();

        var email = dto.Email.Trim().ToLowerInvariant();
        var gymId = ResolveGymIdForRegistration(dto.GymId);

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = _passwordHasher.Hash(dto.Password);
        var user = User.Create(dto.Name.Trim(), email, passwordHash, gymId);

        await _userRepository.AddAsync(user, cancellationToken);

        return MapToResponse(user);
    }

    private void EnsureCanRegisterUsers()
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("Authentication is required to register users.");

        if (!RoleNames.UserRegistrationAllowed.Any(_currentUser.HasRole))
            throw new UnauthorizedAccessException("Only Super Admin or Gym Admin can register users.");
    }

    private Guid? ResolveGymIdForRegistration(Guid? requestedGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return requestedGymId;

        if (_currentUser.HasRole(RoleNames.GymAdmin))
            return _currentUser.RequireGymId();

        throw new UnauthorizedAccessException("Only Super Admin or Gym Admin can register users.");
    }

    private static UserResponseDto MapToResponse(User user) =>
        new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            GymId = user.GymId,
            CreatedDate = user.CreatedDate
        };
}
