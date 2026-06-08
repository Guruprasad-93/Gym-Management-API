using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly IMembershipPlanRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public MembershipPlanService(
        IMembershipPlanRepository repository,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _repository = repository;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<MembershipPlanResponseDto> CreateAsync(CreateMembershipPlanDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymId(dto.GymId);
        var created = await _repository.CreateAsync(gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.MembershipPlan,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<MembershipPlanResponseDto> UpdateAsync(int id, UpdateMembershipPlanDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForPlanAsync(id, cancellationToken);
        var plans = await _repository.GetAllAsync(gymId, true, cancellationToken);
        var existing = plans.First(p => p.Id == id);
        await _repository.UpdateAsync(id, gymId, dto, cancellationToken);
        var updated = (await _repository.GetAllAsync(gymId, true, cancellationToken)).First(p => p.Id == id);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.MembershipPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = existing,
            NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForPlanAsync(id, cancellationToken);
        var plans = await _repository.GetAllAsync(gymId, true, cancellationToken);
        var existing = plans.First(p => p.Id == id);
        await _repository.DeleteAsync(id, gymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.MembershipPlan,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = existing
        }, cancellationToken);
    }

    public Task<IReadOnlyList<MembershipPlanResponseDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(ResolveGymScope(), includeInactive, cancellationToken);

    private async Task<Guid> ResolveGymIdForPlanAsync(int planId, CancellationToken cancellationToken)
    {
        var plan = (await _repository.GetAllAsync(ResolveGymScope(), true, cancellationToken))
            .FirstOrDefault(p => p.Id == planId)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        return ResolveGymIdForEntity(plan.GymId);
    }

    private Guid ResolveGymId(Guid? dtoGymId)
    {
        if (_currentUser.HasRole("SuperAdmin"))
        {
            if (dtoGymId is null) throw new ArgumentException("GymId is required.");
            return dtoGymId.Value;
        }
        return _currentUser.RequireGymId();
    }

    private Guid ResolveGymIdForEntity(Guid entityGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, entityGymId);

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
}
