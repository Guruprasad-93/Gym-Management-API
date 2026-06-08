using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _trainerRepository;
    private readonly IGymRepository _gymRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IMemberRepository _memberRepository;
    private readonly IAuditService _auditService;
    private readonly ITenantLimitService _tenantLimits;

    public TrainerService(
        ITrainerRepository trainerRepository,
        IGymRepository gymRepository,
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        IMemberRepository memberRepository,
        IAuditService auditService,
        ITenantLimitService tenantLimits)
    {
        _trainerRepository = trainerRepository;
        _gymRepository = gymRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _memberRepository = memberRepository;
        _auditService = auditService;
        _tenantLimits = tenantLimits;
    }

    public async Task<TrainerDto> CreateAsync(CreateTrainerDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageTrainers();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        await _tenantLimits.EnsureCanAddTrainerAsync(gymId, cancellationToken);
        _ = await _gymRepository.GetByIdAsync(gymId, cancellationToken)
            ?? throw new KeyNotFoundException("Gym not found.");

        Guid? userId = dto.UserId;
        if (userId is null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Name, email, and password are required when UserId is not provided.");

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
                throw new InvalidOperationException("A user with this email already exists.");

            var user = User.Create(dto.Name.Trim(), email, _passwordHasher.Hash(dto.Password), gymId);
            await _userRepository.AddAsync(user, cancellationToken);
            userId = user.Id;

            var trainerRole = await _roleRepository.GetByNameAsync("Trainer", cancellationToken);
            if (trainerRole is not null)
            {
                var existing = await _userRoleRepository.GetAsync(user.Id, trainerRole.Id, cancellationToken);
                if (existing is null)
                    await _userRoleRepository.AddAsync(UserRole.Create(user.Id, trainerRole.Id), cancellationToken);
            }
        }

        var created = await _trainerRepository.CreateAsync(
            gymId,
            new CreateTrainerDto
            {
                UserId = userId,
                Specialization = dto.Specialization,
                Bio = dto.Bio
            },
            cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Trainer,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<TrainerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(id, cancellationToken);
        return trainer;
    }

    public async Task<TrainerDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasRole("Trainer"))
            throw new UnauthorizedAccessException("Trainer profile not found.");

        return await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Trainer profile not found.");
    }

    public Task<PagedResultDto<TrainerDto>> GetPagedAsync(
        GetTrainersQueryDto query,
        CancellationToken cancellationToken = default)
    {
        EnsureCanManageTrainers();
        var gymId = ResolveGymIdForQuery(query.GymId);
        return _trainerRepository.GetPagedAsync(
            gymId,
            query.Search,
            query.IncludeInactive,
            query.Paging,
            cancellationToken);
    }

    public async Task<TrainerDto> UpdateAsync(int id, UpdateTrainerDto dto, CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageTrainers();
        var gymId = trainer.GymId;
        var oldValue = SnapshotTrainer(trainer);
        await _trainerRepository.UpdateAsync(id, gymId, dto, cancellationToken);
        var updated = (await _trainerRepository.GetByIdAsync(id, ResolveGymScopeForTrainer(trainer.GymId), cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Trainer,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = oldValue,
            NewValue = SnapshotTrainer(updated)
        }, cancellationToken);
        return updated;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageTrainers();
        await _trainerRepository.DeleteAsync(id, trainer.GymId, cancellationToken: cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = trainer.GymId,
            EntityName = AuditEntityNames.Trainer,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = SnapshotTrainer(trainer)
        }, cancellationToken);
    }

    private static object SnapshotTrainer(TrainerDto t) => new
    {
        t.Id,
        t.GymId,
        t.FullName,
        t.Email,
        t.Specialization,
        t.IsActive
    };

    public async Task AssignMembersAsync(int trainerId, AssignMembersToTrainerDto dto, CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(trainerId, cancellationToken);
        EnsureCanManageTrainers();
        foreach (var memberId in dto.MemberIds.Distinct())
            await _trainerRepository.AssignMemberAsync(trainerId, memberId, trainer.GymId, cancellationToken);
    }

    public async Task RemoveMemberAssignmentAsync(int memberId, CancellationToken cancellationToken = default)
    {
        EnsureCanManageTrainers();
        var member = await _memberRepository.GetByIdAsync(memberId, ResolveGymIdForQuery(null), null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        await _trainerRepository.RemoveMemberAssignmentAsync(memberId, member.GymId, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(
        int trainerId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(trainerId, cancellationToken);
        return await _trainerRepository.GetMembersAsync(
            trainerId,
            ResolveGymScopeForTrainer(trainer.GymId),
            search,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MemberDto>> GetUnassignedMembersAsync(
        int trainerId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(trainerId, cancellationToken);
        EnsureCanManageTrainers();
        return await _trainerRepository.GetUnassignedMembersAsync(trainer.GymId, search, cancellationToken);
    }

    public async Task<TrainerDashboardDto> GetDashboardAsync(int trainerId, CancellationToken cancellationToken = default)
    {
        var trainer = await GetTrainerWithAccessCheckAsync(trainerId, cancellationToken);
        var dashboard = await _trainerRepository.GetDashboardAsync(
            trainerId,
            ResolveGymScopeForTrainer(trainer.GymId),
            cancellationToken);

        return dashboard ?? throw new KeyNotFoundException("Trainer dashboard not found.");
    }

    private async Task<TrainerDto> GetTrainerWithAccessCheckAsync(int trainerId, CancellationToken cancellationToken)
    {
        if (IsTrainerOnly())
        {
            var own = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new UnauthorizedAccessException("Trainer profile not found.");
            if (own.Id != trainerId)
                throw new UnauthorizedAccessException("You can only access your own trainer profile.");
            return own;
        }

        var gymScope = ResolveGymScopeForQuery(null);
        return await _trainerRepository.GetByIdAsync(trainerId, gymScope, cancellationToken)
            ?? throw new KeyNotFoundException("Trainer not found.");
    }

    private void EnsureCanManageTrainers()
    {
        if (IsTrainerOnly())
            throw new UnauthorizedAccessException("Trainers cannot manage trainer records.");
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole("Trainer")
        && !_currentUser.HasRole("GymAdmin")
        && !_currentUser.HasRole("SuperAdmin");

    private Guid ResolveGymIdForMutation(Guid? dtoGymId)
    {
        if (_currentUser.HasRole("SuperAdmin"))
        {
            if (dtoGymId is null)
                throw new ArgumentException("GymId is required for platform administrators.");
            return dtoGymId.Value;
        }

        return _currentUser.RequireGymId();
    }

    private Guid ResolveGymIdForQuery(Guid? requestedGymId) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private Guid ResolveGymScopeForTrainer(Guid trainerGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, trainerGymId);

    private Guid ResolveGymScopeForQuery(Guid? requestedGymId) => ResolveGymIdForQuery(requestedGymId);
}
