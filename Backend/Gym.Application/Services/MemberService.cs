using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ITenantLimitService _tenantLimits;

    public MemberService(
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService,
        ITenantLimitService tenantLimits)
    {
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
        _tenantLimits = tenantLimits;
    }

    public async Task<MemberResponseDto> CreateAsync(CreateMemberDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageMembers();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        await _tenantLimits.EnsureCanAddMemberAsync(gymId, cancellationToken);
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        var user = User.Create(dto.Name.Trim(), email, _passwordHasher.Hash(dto.Password), gymId);
        await _userRepository.AddAsync(user, cancellationToken);

        var created = await _memberRepository.CreateAsync(gymId, user.Id, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Member,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new DTOs.Notifications.SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.NewMemberRegistration,
                PhoneNumber = dto.Phone,
                RecipientUserId = user.Id,
                MemberId = created.Id,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = created.FullName,
                    ["email"] = created.Email
                },
                RelatedEntityType = AuditEntityNames.Member,
                RelatedEntityId = created.Id.ToString()
            }, cancellationToken);
        }

        return created;
    }

    public Task<MemberResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetMemberWithAccessCheckAsync(id, cancellationToken);

    public async Task<MemberResponseDto> GetCurrentAsync(CancellationToken cancellationToken = default) =>
        await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
        ?? throw new KeyNotFoundException("Member profile not found.");

    public async Task<PagedResultDto<MemberResponseDto>> GetPagedAsync(
        GetMembersQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymIdForQuery(query.GymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _memberRepository.GetPagedAsync(
            gymId,
            trainerFilter,
            query.Search,
            query.IncludeInactive,
            query.Paging,
            cancellationToken);
    }

    public async Task<MemberDetailsDto> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(id, cancellationToken);
        var gymScope = ResolveGymScopeForDetails(member.GymId);

        IReadOnlyList<MemberPaymentHistoryDto> payments;
        IReadOnlyList<MemberProgressDto> progress;

        try
        {
            payments = await _memberRepository.GetPaymentHistoryAsync(id, gymScope, cancellationToken);
        }
        catch
        {
            payments = Array.Empty<MemberPaymentHistoryDto>();
        }

        try
        {
            progress = await _memberRepository.GetProgressAsync(id, gymScope, cancellationToken);
        }
        catch
        {
            progress = Array.Empty<MemberProgressDto>();
        }

        return new MemberDetailsDto
        {
            Id = member.Id,
            GymId = member.GymId,
            UserId = member.UserId,
            FullName = member.FullName,
            Email = member.Email,
            TrainerId = member.TrainerId,
            TrainerName = member.TrainerName,
            DateOfBirth = member.DateOfBirth,
            Age = member.Age,
            Gender = member.Gender,
            Height = member.Height,
            Weight = member.Weight,
            Phone = member.Phone,
            Address = member.Address,
            EmergencyContact = member.EmergencyContact,
            JoinDate = member.JoinDate,
            IsActive = member.IsActive,
            IsDeleted = member.IsDeleted,
            MembershipStatus = member.MembershipStatus,
            MembershipPlanName = member.MembershipPlanName,
            MembershipEndDate = member.MembershipEndDate,
            CreatedDate = member.CreatedDate,
            UpdatedDate = member.UpdatedDate,
            PaymentHistory = payments,
            Progress = progress
        };
    }

    public async Task<MemberResponseDto> UpdateAsync(int id, UpdateMemberDto dto, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageMembers();
        var oldValue = SnapshotMember(member);
        await _memberRepository.UpdateAsync(id, member.GymId, dto, cancellationToken);
        var updated = (await _memberRepository.GetByIdAsync(id, ResolveGymScopeForMember(member.GymId), null, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = member.GymId,
            EntityName = AuditEntityNames.Member,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = oldValue,
            NewValue = SnapshotMember(updated)
        }, cancellationToken);
        return updated;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageMembers();
        await _memberRepository.DeleteAsync(id, member.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = member.GymId,
            EntityName = AuditEntityNames.Member,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = SnapshotMember(member)
        }, cancellationToken);
    }

    private static object SnapshotMember(MemberResponseDto m) => new
    {
        m.Id,
        m.GymId,
        m.FullName,
        m.Email,
        m.Phone,
        m.TrainerId,
        m.IsActive
    };

    public async Task ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageMembers();
        await _memberRepository.ActivateAsync(id, member.GymId, cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(id, cancellationToken);
        EnsureCanManageMembers();
        await _memberRepository.DeactivateAsync(id, member.GymId, cancellationToken);
    }

    public async Task AssignTrainerAsync(int memberId, AssignTrainerToMemberDto dto, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(memberId, cancellationToken);
        EnsureCanManageMembers();
        await _memberRepository.AssignTrainerAsync(memberId, dto.TrainerId, member.GymId, cancellationToken);
    }

    public async Task RemoveTrainerAssignmentAsync(int memberId, CancellationToken cancellationToken = default)
    {
        var member = await GetMemberWithAccessCheckAsync(memberId, cancellationToken);
        EnsureCanManageMembers();
        await _memberRepository.RemoveTrainerAssignmentAsync(memberId, member.GymId, cancellationToken);
    }

    private async Task<MemberResponseDto> GetMemberWithAccessCheckAsync(int memberId, CancellationToken cancellationToken)
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
        var gymScope = ResolveGymIdForQuery(null);

        return await _memberRepository.GetByIdAsync(memberId, gymScope, trainerFilter, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
    }

    private async Task<int?> ResolveTrainerFilterAsync(CancellationToken cancellationToken)
    {
        if (!IsTrainerOnly())
            return null;

        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
        return trainer.Id;
    }

    private void EnsureCanManageMembers()
    {
        if (IsTrainerOnly())
            throw new UnauthorizedAccessException("Trainers cannot manage member records.");
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole("Trainer")
        && !_currentUser.HasRole("GymAdmin")
        && !_currentUser.HasRole("SuperAdmin");

    private bool IsMemberOnly() =>
        _currentUser.HasRole("Member")
        && !_currentUser.HasRole("GymAdmin")
        && !_currentUser.HasRole("SuperAdmin")
        && !_currentUser.HasRole("Trainer");

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

    private Guid ResolveGymScopeForMember(Guid memberGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, memberGymId);

    private Guid ResolveGymScopeForDetails(Guid memberGymId) =>
        ResolveGymScopeForMember(memberGymId);
}
