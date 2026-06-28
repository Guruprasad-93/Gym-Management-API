using Gym.Application.DTOs.Users;
using Gym.Application.Interfaces;
using Gym.Application.Validation;
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

        var loginIdentifier = LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        LoginIdentifierRules.Validate(loginIdentifier);
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        var gymId = ResolveGymIdForRegistration(dto.GymId);

        if (await _userRepository.ExistsByLoginIdentifierAsync(loginIdentifier, cancellationToken))
            throw new InvalidOperationException("A user with this login identifier already exists.");

        if (!string.IsNullOrWhiteSpace(email) && await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = _passwordHasher.Hash(dto.Password);
        var user = User.Create(dto.Name.Trim(), loginIdentifier, passwordHash, gymId, email);

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
            LoginIdentifier = user.LoginIdentifier,
            Email = user.Email ?? string.Empty,
            GymId = user.GymId,
            CreatedDate = user.CreatedDate
        };
}
