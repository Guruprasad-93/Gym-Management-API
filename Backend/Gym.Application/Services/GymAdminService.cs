using Gym.Application.Authorization;
using Gym.Application.Common;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class GymAdminService : IGymAdminService
{
    private readonly IGymRepository _gymRepository;
    private readonly IGymAdminRepository _gymAdminRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public GymAdminService(
        IGymRepository gymRepository,
        IGymAdminRepository gymAdminRepository,
        IUserRepository userRepository,
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser)
    {
        _gymRepository = gymRepository;
        _gymAdminRepository = gymAdminRepository;
        _userRepository = userRepository;
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<CreateGymAdminResultDto> CreateAsync(
        CreateGymAdminDto dto,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymIdForMutation(dto.GymId);
        _ = await _gymRepository.GetByIdAsync(gymId, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");

        var loginIdentifier = Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByLoginIdentifierAsync(loginIdentifier, gymId, cancellationToken))
            throw new InvalidOperationException("A user with this login identifier already exists.");

        if (!string.IsNullOrWhiteSpace(email) && await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        string? temporaryPassword = null;
        var mustChangePassword = false;
        string plainPassword;

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            if (!dto.GenerateTemporaryPassword)
                throw new ArgumentException("Password is required when temporary password generation is disabled.");

            temporaryPassword = TemporaryPasswordGenerator.Generate();
            plainPassword = temporaryPassword;
            mustChangePassword = true;
        }
        else
        {
            plainPassword = dto.Password;
            mustChangePassword = dto.GenerateTemporaryPassword;
            if (dto.GenerateTemporaryPassword)
                temporaryPassword = plainPassword;
        }

        var userId = Guid.NewGuid();
        await _gymAdminRepository.CreateAsync(
            userId,
            gymId,
            dto.Name.Trim(),
            loginIdentifier,
            email,
            _passwordHasher.Hash(plainPassword),
            mustChangePassword,
            cancellationToken);

        var admin = await _gymAdminRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to load created gym admin.");

        return new CreateGymAdminResultDto
        {
            Admin = admin,
            TemporaryPassword = temporaryPassword,
            Message = temporaryPassword is not null
                ? "Gym admin created. Share the temporary password securely with the user."
                : "Gym admin created successfully."
        };
    }

    public Task<PagedResultDto<GymAdminDto>> GetAllAsync(
        Guid? gymId,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default) =>
        _gymAdminRepository.GetAllAsync(ResolveGymIdForQuery(gymId), paging, cancellationToken);

    public async Task<GymAdminDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var admin = await _gymAdminRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Gym admin not found.");

        EnsureCanAccessGymAdmin(admin);
        return admin;
    }

    public async Task<GymAdminDto> UpdateAsync(
        Guid userId,
        UpdateGymAdminDto dto,
        CancellationToken cancellationToken = default)
    {
        _ = await GetByIdAsync(userId, cancellationToken);

        var gymId = ResolveGymIdForMutation(dto.GymId);
        _ = await _gymRepository.GetByIdAsync(gymId, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");

        var loginIdentifier = Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByLoginIdentifierAsync(loginIdentifier, gymId, cancellationToken))
        {
            var conflict = await _userRepository.GetByLoginIdentifierAsync(loginIdentifier, gymId, cancellationToken);
            if (conflict is not null && conflict.Id != userId)
                throw new InvalidOperationException("A user with this login identifier already exists.");
        }

        if (!string.IsNullOrWhiteSpace(email) && await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            var emailUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (emailUser is not null && emailUser.Id != userId)
                throw new InvalidOperationException("A user with this email already exists.");
        }

        var updateDto = new UpdateGymAdminDto
        {
            Name = dto.Name.Trim(),
            LoginIdentifier = loginIdentifier,
            Email = email,
            GymId = gymId
        };

        await _gymAdminRepository.UpdateAsync(userId, updateDto, cancellationToken);
        return await GetByIdAsync(userId, cancellationToken);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        _ = await GetByIdAsync(userId, cancellationToken);
        await _gymAdminRepository.SetActiveAsync(userId, isActive, cancellationToken);

        if (!isActive)
        {
            await _authRepository.EndAllSessionsForUserAsync(userId, cancellationToken);
            await _authRepository.RevokeAllRefreshTokensForUserAsync(userId, cancellationToken);
        }
    }

    public async Task<ResendTemporaryPasswordResultDto> ResendTemporaryPasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var admin = await GetByIdAsync(userId, cancellationToken);
        var temporaryPassword = TemporaryPasswordGenerator.Generate();

        await _gymAdminRepository.ResetPasswordAsync(
            userId,
            _passwordHasher.Hash(temporaryPassword),
            mustChangePassword: true,
            cancellationToken);

        await _authRepository.EndAllSessionsForUserAsync(userId, cancellationToken);
        await _authRepository.RevokeAllRefreshTokensForUserAsync(userId, cancellationToken);

        return new ResendTemporaryPasswordResultDto
        {
            UserId = userId,
            Email = admin.Email,
            TemporaryPassword = temporaryPassword,
            Message = "A new temporary password was generated. Share it securely with the gym admin."
        };
    }

    private void EnsureCanAccessGymAdmin(GymAdminDto admin)
    {
        if (_currentUser.HasRole("SuperAdmin"))
            return;

        if (admin.GymId != _currentUser.RequireGymId())
            throw new KeyNotFoundException("Gym admin not found.");
    }

    private Guid ResolveGymIdForMutation(Guid dtoGymId)
    {
        if (_currentUser.HasRole("SuperAdmin"))
            return dtoGymId;

        var gymId = _currentUser.RequireGymId();
        if (dtoGymId != Guid.Empty && dtoGymId != gymId)
            throw new UnauthorizedAccessException("Cannot manage resources for another gym.");

        return gymId;
    }

    private Guid? ResolveGymIdForQuery(Guid? requestedGymId)
    {
        if (_currentUser.HasRole("SuperAdmin"))
            return requestedGymId is null || requestedGymId == Guid.Empty ? null : requestedGymId;

        return GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
    }
}
