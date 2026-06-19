using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class GymService : IGymService
{
    private readonly IGymRepository _gymRepository;
    private readonly ISaasSubscriptionRepository _saasRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IDietPlanRepository _dietPlanRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IGymMenuService _gymMenuService;
    private readonly SaasSubscriptionSettings _saasSettings;

    public GymService(
        IGymRepository gymRepository,
        ISaasSubscriptionRepository saasRepository,
        IExpenseRepository expenseRepository,
        IDietPlanRepository dietPlanRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IGymMenuService gymMenuService,
        IOptions<SaasSubscriptionSettings> saasSettings)
    {
        _gymRepository = gymRepository;
        _saasRepository = saasRepository;
        _expenseRepository = expenseRepository;
        _dietPlanRepository = dietPlanRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _gymMenuService = gymMenuService;
        _saasSettings = saasSettings.Value;
    }

    public async Task<GymDto> CreateAsync(CreateGymDto dto, CancellationToken cancellationToken = default)
    {
        var gym = Gym.Domain.Entities.Gym.Create(dto.Name, dto.Address, dto.Phone, dto.Email);
        var created = await _gymRepository.CreateAsync(gym.Id, dto, cancellationToken);
        if (await _saasRepository.GetGymSubscriptionAsync(gym.Id, cancellationToken) is null)
            await _saasRepository.CreateTrialSubscriptionAsync(gym.Id, _saasSettings.GracePeriodDays, cancellationToken);
        await _expenseRepository.SeedCategoriesAsync(gym.Id, cancellationToken);
        await _dietPlanRepository.SeedCategoriesAsync(gym.Id, cancellationToken);
        await _workoutPlanRepository.SeedExerciseCategoriesAsync(gym.Id, cancellationToken);
        await _workoutPlanRepository.SeedExerciseLibraryAsync(gym.Id, cancellationToken);
        await _gymMenuService.SeedMenusForGymAsync(gym.Id, null, cancellationToken);
        return created;
    }

    public Task<GymDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _gymRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<GymDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _gymRepository.GetAllAsync(cancellationToken);

    public async Task<GymDto> UpdateAsync(Guid id, UpdateGymDto dto, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.UpdateAsync(id, dto, cancellationToken);
        return (await _gymRepository.GetByIdAsync(id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        _ = await _gymRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");
        await _gymRepository.SetActiveAsync(id, isActive, cancellationToken);
    }
}
