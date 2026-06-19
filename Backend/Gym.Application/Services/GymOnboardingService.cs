using Gym.Application.Authorization;
using Gym.Application.Common;
using Gym.Application.Constants;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.DTOs.Gyms;
using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class GymOnboardingService : IGymOnboardingService
{
    private readonly IGymRepository _gymRepository;
    private readonly IGymAdminRepository _gymAdminRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISaasSubscriptionRepository _saasRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IDietPlanRepository _dietPlanRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IGymMenuService _gymMenuService;
    private readonly INotificationService _notificationService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SaasSubscriptionSettings _settings;

    public GymOnboardingService(
        IGymRepository gymRepository,
        IGymAdminRepository gymAdminRepository,
        IUserRepository userRepository,
        ISaasSubscriptionRepository saasRepository,
        IExpenseRepository expenseRepository,
        IDietPlanRepository dietPlanRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IGymMenuService gymMenuService,
        INotificationService notificationService,
        IPasswordHasher passwordHasher,
        IOptions<SaasSubscriptionSettings> settings)
    {
        _gymRepository = gymRepository;
        _gymAdminRepository = gymAdminRepository;
        _userRepository = userRepository;
        _saasRepository = saasRepository;
        _expenseRepository = expenseRepository;
        _dietPlanRepository = dietPlanRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _gymMenuService = gymMenuService;
        _notificationService = notificationService;
        _passwordHasher = passwordHasher;
        _settings = settings.Value;
    }

    public async Task<RegisterGymResultDto> RegisterGymAsync(RegisterGymDto dto, CancellationToken cancellationToken = default)
    {
        if (!_settings.AllowPublicRegistration)
            throw new InvalidOperationException("Public gym registration is disabled.");

        var loginIdentifier = Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier);
        Validation.LoginIdentifierRules.Validate(loginIdentifier);
        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByLoginIdentifierAsync(loginIdentifier, null, cancellationToken))
            throw new InvalidOperationException("A user with this login identifier already exists.");

        if (!string.IsNullOrWhiteSpace(email) && await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var gymId = Guid.NewGuid();
        await _gymRepository.CreateAsync(gymId, new CreateGymDto
        {
            Name = dto.GymName.Trim(),
            Address = dto.Address?.Trim(),
            Phone = dto.Mobile.Trim(),
            Email = email
        }, cancellationToken);

        string? temporaryPassword = null;
        var plainPassword = string.IsNullOrWhiteSpace(dto.Password)
            ? TemporaryPasswordGenerator.Generate()
            : dto.Password;
        if (string.IsNullOrWhiteSpace(dto.Password))
            temporaryPassword = plainPassword;

        var userId = Guid.NewGuid();
        await _gymAdminRepository.CreateAsync(
            userId, gymId, dto.OwnerName.Trim(), loginIdentifier, email,
            _passwordHasher.Hash(plainPassword), temporaryPassword is not null, cancellationToken);

        await _saasRepository.CreateTrialSubscriptionAsync(gymId, _settings.GracePeriodDays, cancellationToken);
        await _saasRepository.SeedNotificationSettingsAsync(gymId, cancellationToken);
        await _expenseRepository.SeedCategoriesAsync(gymId, cancellationToken);
        await _dietPlanRepository.SeedCategoriesAsync(gymId, cancellationToken);
        await _workoutPlanRepository.SeedExerciseCategoriesAsync(gymId, cancellationToken);
        await _workoutPlanRepository.SeedExerciseLibraryAsync(gymId, cancellationToken);
        await _gymMenuService.SeedMenusForGymAsync(gymId, userId, cancellationToken);

        var subscription = await _saasRepository.GetGymSubscriptionAsync(gymId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Mobile))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.GymOwnerWelcome,
                PhoneNumber = dto.Mobile.Trim(),
                RecipientUserId = userId,
                Variables = new Dictionary<string, string>
                {
                    ["ownerName"] = dto.OwnerName.Trim(),
                    ["gymName"] = dto.GymName.Trim(),
                    ["email"] = email,
                    ["trialDays"] = (_settings.TrialDays).ToString()
                },
                RelatedEntityType = AuditEntityNames.Gym,
                RelatedEntityId = gymId.ToString()
            }, cancellationToken);
        }

        return new RegisterGymResultDto
        {
            GymId = gymId,
            AdminUserId = userId,
            GymName = dto.GymName.Trim(),
            AdminLoginIdentifier = loginIdentifier,
            AdminEmail = email,
            TemporaryPassword = temporaryPassword,
            RemainingTrialDays = subscription?.RemainingTrialDays ?? _settings.TrialDays,
            Message = temporaryPassword is not null
                ? "Gym registered successfully. Use the temporary password to log in and change it."
                : "Gym registered successfully. You can log in with your credentials."
        };
    }
}
