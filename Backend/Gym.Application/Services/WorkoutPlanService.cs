using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.WorkoutPlans;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly IWorkoutPlanRepository _repository;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public WorkoutPlanService(
        IWorkoutPlanRepository repository,
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<ExerciseCategoryDto>> GetCategoriesAsync(
        bool includeInactive, CancellationToken cancellationToken = default) =>
        await _repository.GetCategoriesAsync(ResolveGymIdForMutation(null), includeInactive, cancellationToken);

    public async Task<ExerciseCategoryDto> CreateCategoryAsync(
        CreateExerciseCategoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        return await _repository.CreateCategoryAsync(ResolveGymIdForMutation(null), dto, cancellationToken);
    }

    public Task<IReadOnlyList<ExerciseDto>> GetExercisesAsync(
        bool includeInactive, int? categoryId, string? muscleGroup, string? search, CancellationToken cancellationToken = default) =>
        _repository.GetExercisesAsync(ResolveGymScope(), includeInactive, categoryId, muscleGroup, search, cancellationToken);

    public async Task<ExerciseDto> GetExerciseByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _repository.GetExerciseByIdAsync(id, ResolveGymScope(), cancellationToken)
        ?? throw new KeyNotFoundException("Exercise not found.");

    public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        var id = await _repository.CreateExerciseAsync(gymId, dto, cancellationToken);
        var created = await GetExerciseByIdAsync(id, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Exercise,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetExerciseByIdAsync(id, cancellationToken);
        await _repository.UpdateExerciseAsync(id, existing.GymId, dto, cancellationToken);
        var updated = await GetExerciseByIdAsync(id, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.Exercise,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = existing,
            NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task DeleteExerciseAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetExerciseByIdAsync(id, cancellationToken);
        await _repository.DeleteExerciseAsync(id, existing.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.Exercise,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = existing
        }, cancellationToken);
    }

    public Task<IReadOnlyList<WorkoutPlanListDto>> GetPlansAsync(
        bool includeInactive, string? search, CancellationToken cancellationToken = default) =>
        _repository.GetPlansAsync(ResolveGymScope(), includeInactive, search, cancellationToken);

    public async Task<WorkoutPlanDetailDto> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await GetPlanOrThrowAsync(id, cancellationToken);

    public async Task<WorkoutPlanDetailDto> CreatePlanAsync(
        CreateWorkoutPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        var planId = await _repository.CreatePlanAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        var created = (await _repository.GetPlanByIdAsync(planId, gymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.WorkoutPlan,
            EntityId = planId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<WorkoutPlanDetailDto> UpdatePlanAsync(
        int id, UpdateWorkoutPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        await _repository.UpdatePlanAsync(id, existing.GymId, dto, cancellationToken);
        var updated = (await _repository.GetPlanByIdAsync(id, existing.GymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.WorkoutPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = existing,
            NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task DeletePlanAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        await _repository.DeletePlanAsync(id, existing.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.WorkoutPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = existing
        }, cancellationToken);
    }

    public async Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        await _repository.SetActiveAsync(id, existing.GymId, isActive, cancellationToken);
    }

    public async Task<WorkoutPlanDetailDto> ClonePlanAsync(
        int id, CloneWorkoutPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var source = await GetPlanOrThrowAsync(id, cancellationToken);
        var newId = await _repository.ClonePlanAsync(id, source.GymId, dto.NewPlanName, _currentUser.UserId, cancellationToken);
        var cloned = (await _repository.GetPlanByIdAsync(newId, source.GymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = source.GymId,
            EntityName = AuditEntityNames.WorkoutPlan,
            EntityId = newId.ToString(),
            ActionType = AuditActionTypes.Clone,
            OldValue = new { SourceWorkoutPlanId = id },
            NewValue = cloned
        }, cancellationToken);
        return cloned;
    }

    public async Task<MemberWorkoutPlanViewDto> AssignToMemberAsync(
        AssignWorkoutPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanAssign();
        var member = await EnsureMemberAccessAsync(dto.MemberId, cancellationToken);
        var gymId = ResolveGymIdForEntity(member.GymId);
        var plan = await GetPlanOrThrowAsync(dto.WorkoutPlanId, cancellationToken);
        var assignedId = await _repository.AssignToMemberAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        var view = await GetMemberWorkoutWithCompletionAsync(dto.MemberId, gymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.AssignedWorkoutPlan,
            EntityId = assignedId.ToString(),
            ActionType = AuditActionTypes.Assign,
            NewValue = new { dto.MemberId, dto.WorkoutPlanId, dto.StartDate, dto.EndDate }
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(member.Phone))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.WorkoutPlanAssigned,
                PhoneNumber = member.Phone,
                RecipientUserId = member.UserId,
                MemberId = member.Id,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = member.FullName,
                    ["planName"] = plan.PlanName
                },
                RelatedEntityType = AuditEntityNames.AssignedWorkoutPlan,
                RelatedEntityId = assignedId.ToString()
            }, cancellationToken);
        }

        return view;
    }

    public async Task UnassignAsync(int assignedWorkoutPlanId, CancellationToken cancellationToken = default)
    {
        EnsureCanAssign();
        var gymId = _currentUser.HasRole(RoleNames.SuperAdmin)
            ? throw new UnauthorizedAccessException("Unassign requires gym context.")
            : _currentUser.RequireGymId();
        await _repository.UnassignAsync(assignedWorkoutPlanId, gymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.AssignedWorkoutPlan,
            EntityId = assignedWorkoutPlanId.ToString(),
            ActionType = AuditActionTypes.Delete
        }, cancellationToken);
    }

    public async Task<MemberWorkoutPlanViewDto> GetMemberWorkoutAsync(
        int memberId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAccessAsync(memberId, cancellationToken);
        return await GetMemberWorkoutWithCompletionAsync(memberId, ResolveGymScope(), cancellationToken);
    }

    public async Task<MemberWorkoutPlanViewDto> GetCurrentMemberWorkoutAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasRole(RoleNames.Member))
            throw new UnauthorizedAccessException("Member profile required.");
        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
        return await GetMemberWorkoutWithCompletionAsync(member.Id, member.GymId, cancellationToken);
    }

    public async Task<WorkoutPlanExerciseDto> UpdateProgressAsync(
        UpdateWorkoutProgressDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.HasRole(RoleNames.SuperAdmin)
            ? throw new UnauthorizedAccessException("Progress update requires gym context.")
            : _currentUser.RequireGymId();

        if (IsMemberOnly())
        {
            var own = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            dto.MemberId = own.Id;
        }

        await EnsureMemberAccessAsync(dto.MemberId, cancellationToken);
        var workout = await GetMemberWorkoutWithCompletionAsync(dto.MemberId, gymId, cancellationToken);
        if (workout.AssignedWorkoutPlanId != dto.AssignedWorkoutPlanId)
            throw new KeyNotFoundException("Workout assignment not found.");

        if (IsMemberOnly())
        {
            dto = new UpdateWorkoutProgressDto
            {
                AssignedWorkoutPlanId = dto.AssignedWorkoutPlanId,
                WorkoutPlanExerciseId = dto.WorkoutPlanExerciseId,
                IsCompleted = dto.IsCompleted,
                CompletionPercentage = dto.CompletionPercentage ?? (dto.IsCompleted == true ? 100 : dto.CompletionPercentage),
                MemberNotes = dto.MemberNotes,
                TrainerNotes = null
            };
        }

        if (dto.IsCompleted == true && dto.CompletionPercentage is null)
            dto.CompletionPercentage = 100;

        await _repository.UpsertProgressAsync(gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.MemberWorkoutProgress,
            EntityId = $"{dto.AssignedWorkoutPlanId}:{dto.WorkoutPlanExerciseId}",
            ActionType = AuditActionTypes.Update,
            NewValue = dto
        }, cancellationToken);

        var updated = await GetMemberWorkoutWithCompletionAsync(dto.MemberId, gymId, cancellationToken);
        return updated.Exercises.First(e => e.WorkoutPlanExerciseId == dto.WorkoutPlanExerciseId);
    }

    public Task<WorkoutPlanDetailDto> GetPlanForExportAsync(int id, CancellationToken cancellationToken = default) =>
        GetPlanOrThrowAsync(id, cancellationToken);

    private async Task<MemberWorkoutPlanViewDto> GetMemberWorkoutWithCompletionAsync(
        int memberId, Guid? gymScope, CancellationToken cancellationToken)
    {
        var view = await _repository.GetMemberWorkoutAsync(memberId, gymScope, true, cancellationToken);
        if (view.Exercises.Count == 0)
        {
            view.OverallCompletionPercentage = 0;
            return view;
        }

        var total = view.Exercises.Sum(e => e.CompletionPercentage ?? (e.IsCompleted == true ? 100m : 0m));
        view.OverallCompletionPercentage = Math.Round(total / view.Exercises.Count, 1);
        return view;
    }

    private async Task<WorkoutPlanDetailDto> GetPlanOrThrowAsync(int id, CancellationToken cancellationToken) =>
        await _repository.GetPlanByIdAsync(id, ResolveGymScope(), cancellationToken)
        ?? throw new KeyNotFoundException("Workout plan not found.");

    private async Task<Application.DTOs.Members.MemberResponseDto> EnsureMemberAccessAsync(
        int memberId, CancellationToken cancellationToken)
    {
        if (IsMemberOnly())
        {
            var own = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            if (own.Id != memberId)
                throw new KeyNotFoundException("Member not found.");
            return own;
        }

        int? trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _memberRepository.GetByIdAsync(memberId, ResolveGymScope(), trainerFilter, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
    }

    private async Task<int?> ResolveTrainerFilterAsync(CancellationToken cancellationToken)
    {
        if (!IsTrainerOnly()) return null;
        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
        return trainer.Id;
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManageWorkoutPlans))
            throw new UnauthorizedAccessException("Insufficient permissions to manage workout plans.");
    }

    private void EnsureCanAssign()
    {
        if (IsMemberOnly())
            throw new UnauthorizedAccessException("Members cannot assign workout plans.");
        if (!_currentUser.HasPermission(Permissions.AssignWorkoutPlan)
            && !_currentUser.HasPermission(Permissions.ManageWorkoutPlans))
            throw new UnauthorizedAccessException("Insufficient permissions to assign workout plans.");
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole(RoleNames.Trainer)
        && !_currentUser.HasRole(RoleNames.GymAdmin)
        && !_currentUser.HasRole(RoleNames.SuperAdmin);

    private bool IsMemberOnly() =>
        _currentUser.HasRole(RoleNames.Member)
        && !_currentUser.HasRole(RoleNames.GymAdmin)
        && !_currentUser.HasRole(RoleNames.SuperAdmin)
        && !_currentUser.HasRole(RoleNames.Trainer);

    private Guid ResolveGymIdForMutation(Guid? dtoGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
        {
            if (dtoGymId is null) throw new ArgumentException("GymId is required for platform administrators.");
            return dtoGymId.Value;
        }
        return _currentUser.RequireGymId();
    }

    private Guid ResolveGymIdForEntity(Guid entityGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, entityGymId);

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
}
