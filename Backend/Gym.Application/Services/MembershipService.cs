using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public MembershipService(
        IMembershipRepository membershipRepository,
        IMemberRepository memberRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _membershipRepository = membershipRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<MembershipResponseDto> CreateAsync(CreateMembershipDto dto, CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByIdAsync(dto.MemberId, ResolveGymScope(), null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        var gymId = ResolveGymIdForEntity(member.GymId);
        var created = await _membershipRepository.CreateAsync(gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Membership,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<MembershipResponseDto> RenewAsync(int id, RenewMembershipDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _membershipRepository.GetByIdAsync(id, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Membership not found.");

        var gymId = ResolveGymIdForEntity(existing.GymId);
        await _membershipRepository.RenewAsync(id, gymId, dto, cancellationToken);
        var list = await _membershipRepository.GetAllAsync(gymId, existing.MemberId, null, true, cancellationToken);
        var renewed = list.OrderByDescending(m => m.StartDate).First();
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Membership,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Renew,
            OldValue = existing,
            NewValue = renewed
        }, cancellationToken);

        var member = await _memberRepository.GetByIdAsync(existing.MemberId, gymId, null, cancellationToken);
        if (member?.Phone is { Length: > 0 } phone)
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.MembershipRenewal,
                PhoneNumber = phone,
                RecipientUserId = member.UserId,
                MemberId = member.Id,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = member.FullName,
                    ["planName"] = renewed.PlanName,
                    ["endDate"] = renewed.EndDate.ToString("yyyy-MM-dd")
                },
                RelatedEntityType = AuditEntityNames.Membership,
                RelatedEntityId = renewed.Id.ToString()
            }, cancellationToken);
        }

        return renewed;
    }

    public async Task CancelAsync(int id, CancelMembershipDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _membershipRepository.GetByIdAsync(id, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Membership not found.");

        var gymId = ResolveGymIdForEntity(existing.GymId);
        await _membershipRepository.CancelAsync(id, gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Membership,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Cancel,
            OldValue = existing,
            NewValue = new { dto.Notes }
        }, cancellationToken);
    }

    public Task<MembershipResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetMembershipAsync(id, cancellationToken);

    public Task<IReadOnlyList<MembershipResponseDto>> GetAllAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default) =>
        _membershipRepository.GetAllAsync(ResolveGymScope(), null, search, includeInactive, cancellationToken);

    public Task<IReadOnlyList<MembershipResponseDto>> GetExpiredAsync(CancellationToken cancellationToken = default) =>
        _membershipRepository.GetExpiredAsync(ResolveGymScope(), cancellationToken);

    private async Task<MembershipResponseDto> GetMembershipAsync(int id, CancellationToken cancellationToken) =>
        await _membershipRepository.GetByIdAsync(id, ResolveGymScope(), cancellationToken)
        ?? throw new KeyNotFoundException("Membership not found.");

    private Guid ResolveGymIdForEntity(Guid entityGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, entityGymId);

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
}
