using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.DietPlans;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class DietPlanService : IDietPlanService
{
    private readonly IDietPlanRepository _repository;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public DietPlanService(
        IDietPlanRepository repository,
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

    public async Task<IReadOnlyList<DietCategoryDto>> GetCategoriesAsync(
        bool includeInactive, CancellationToken cancellationToken = default) =>
        await _repository.GetCategoriesAsync(ResolveGymIdForMutation(null), includeInactive, cancellationToken);

    public async Task<DietCategoryDto> CreateCategoryAsync(
        CreateDietCategoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        return await _repository.CreateCategoryAsync(ResolveGymIdForMutation(null), dto, cancellationToken);
    }

    public Task<IReadOnlyList<DietPlanListDto>> GetPlansAsync(
        bool includeInactive, int? categoryId, string? search, CancellationToken cancellationToken = default) =>
        _repository.GetPlansAsync(ResolveGymScope(), includeInactive, categoryId, search, cancellationToken);

    public async Task<DietPlanDetailDto> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await GetPlanOrThrowAsync(id, cancellationToken);

    public async Task<DietPlanDetailDto> CreatePlanAsync(
        CreateDietPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        var planId = await _repository.CreatePlanAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        var created = (await _repository.GetPlanByIdAsync(planId, gymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.DietPlan,
            EntityId = planId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<DietPlanDetailDto> UpdatePlanAsync(
        int id, UpdateDietPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        var gymId = existing.GymId;
        await _repository.UpdatePlanAsync(id, gymId, dto, cancellationToken);
        var updated = (await _repository.GetPlanByIdAsync(id, gymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.DietPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = existing,
            NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task DeletePlanAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        await _repository.DeletePlanAsync(id, existing.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.DietPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = existing
        }, cancellationToken);
    }

    public async Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        var existing = await GetPlanOrThrowAsync(id, cancellationToken);
        await _repository.SetActiveAsync(id, existing.GymId, isActive, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.DietPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = new { existing.IsActive },
            NewValue = new { IsActive = isActive }
        }, cancellationToken);
    }

    public async Task<DietPlanDetailDto> ClonePlanAsync(
        int id, CloneDietPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManagePlans();
        var source = await GetPlanOrThrowAsync(id, cancellationToken);
        var newId = await _repository.ClonePlanAsync(id, source.GymId, dto.NewPlanName, _currentUser.UserId, cancellationToken);
        var cloned = (await _repository.GetPlanByIdAsync(newId, source.GymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = source.GymId,
            EntityName = AuditEntityNames.DietPlan,
            EntityId = newId.ToString(),
            ActionType = AuditActionTypes.Clone,
            OldValue = new { SourceDietPlanId = id },
            NewValue = cloned
        }, cancellationToken);
        return cloned;
    }

    public async Task<MemberDietPlanViewDto> AssignToMemberAsync(
        AssignDietPlanDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanAssign();
        var member = await EnsureMemberAccessAsync(dto.MemberId, cancellationToken);
        var gymId = ResolveGymIdForEntity(member.GymId);
        var plan = await GetPlanOrThrowAsync(dto.DietPlanId, cancellationToken);

        var assignedId = await _repository.AssignToMemberAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        var view = await _repository.GetMemberDietAsync(dto.MemberId, gymId, true, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.AssignedDietPlan,
            EntityId = assignedId.ToString(),
            ActionType = AuditActionTypes.Assign,
            NewValue = new { dto.MemberId, dto.DietPlanId, dto.StartDate, dto.EndDate }
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(member.Phone))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.DietPlanAssigned,
                PhoneNumber = member.Phone,
                RecipientUserId = member.UserId,
                MemberId = member.Id,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = member.FullName,
                    ["planName"] = plan.PlanName
                },
                RelatedEntityType = AuditEntityNames.AssignedDietPlan,
                RelatedEntityId = assignedId.ToString()
            }, cancellationToken);
        }

        return view;
    }

    public async Task UnassignAsync(int assignedDietPlanId, CancellationToken cancellationToken = default)
    {
        EnsureCanAssign();
        var gymId = _currentUser.HasRole(RoleNames.SuperAdmin)
            ? throw new UnauthorizedAccessException("Unassign is not supported for platform administrators without gym context.")
            : _currentUser.RequireGymId();

        await _repository.UnassignAsync(assignedDietPlanId, gymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.AssignedDietPlan,
            EntityId = assignedDietPlanId.ToString(),
            ActionType = AuditActionTypes.Delete
        }, cancellationToken);
    }

    public async Task<MemberDietPlanViewDto> GetMemberDietAsync(
        int memberId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAccessAsync(memberId, cancellationToken);
        var gymScope = ResolveGymScopeForMemberQuery();
        return await _repository.GetMemberDietAsync(memberId, gymScope, true, cancellationToken);
    }

    public async Task<MemberDietPlanViewDto> GetCurrentMemberDietAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasRole(RoleNames.Member))
            throw new UnauthorizedAccessException("Member profile required.");

        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        return await _repository.GetMemberDietAsync(member.Id, member.GymId, true, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberDietAssignmentDto>> GetMemberAssignmentsAsync(
        int memberId, CancellationToken cancellationToken = default)
    {
        var member = await EnsureMemberAccessAsync(memberId, cancellationToken);
        var gymId = ResolveGymIdForEntity(member.GymId);
        return await _repository.GetMemberAssignmentsAsync(memberId, gymId, cancellationToken);
    }

    public Task<DietPlanDetailDto> GetPlanForExportAsync(int id, CancellationToken cancellationToken = default) =>
        GetPlanOrThrowAsync(id, cancellationToken);

    private async Task<DietPlanDetailDto> GetPlanOrThrowAsync(int id, CancellationToken cancellationToken)
    {
        var gymScope = ResolveGymScope();
        return await _repository.GetPlanByIdAsync(id, gymScope, cancellationToken)
            ?? throw new KeyNotFoundException("Diet plan not found.");
    }

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
        var gymScope = ResolveGymScope();
        return await _memberRepository.GetByIdAsync(memberId, gymScope, trainerFilter, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
    }

    private async Task<int?> ResolveTrainerFilterAsync(CancellationToken cancellationToken)
    {
        if (!IsTrainerOnly()) return null;
        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
        return trainer.Id;
    }

    private void EnsureCanManagePlans()
    {
        if (IsMemberOnly() || IsTrainerOnly())
            throw new UnauthorizedAccessException("Insufficient permissions to manage diet plans.");
    }

    private void EnsureCanAssign()
    {
        if (IsMemberOnly())
            throw new UnauthorizedAccessException("Members cannot assign diet plans.");
        if (!_currentUser.HasPermission(Permissions.AssignDietPlan)
            && !_currentUser.HasPermission(Permissions.ManageDietPlans))
            throw new UnauthorizedAccessException("Insufficient permissions to assign diet plans.");
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
            if (dtoGymId is null)
                throw new ArgumentException("GymId is required for platform administrators.");
            return dtoGymId.Value;
        }
        return _currentUser.RequireGymId();
    }

    private Guid ResolveGymIdForEntity(Guid entityGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, entityGymId);

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private Guid ResolveGymScopeForMemberQuery(Guid? requestedGymId = null) => ResolveGymScope(requestedGymId);
}
