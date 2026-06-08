using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repository;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public BranchService(
        IBranchRepository repository,
        IAuditService auditService,
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _auditService = auditService;
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        var created = await _repository.CreateAsync(gymId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.Branch, created.BranchId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        return created;
    }

    public async Task UpdateAsync(int branchId, UpdateBranchDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        await _repository.UpdateAsync(branchId, gymId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.Branch, branchId.ToString(), AuditActionTypes.Update, dto, cancellationToken);
    }

    public async Task SetActiveAsync(int branchId, bool isActive, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        await _repository.SetActiveAsync(branchId, gymId, isActive, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.Branch, branchId.ToString(), isActive ? AuditActionTypes.Activate : AuditActionTypes.Deactivate, null, cancellationToken);
    }

    public async Task DeleteAsync(int branchId, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        await _repository.DeleteAsync(branchId, gymId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.Branch, branchId.ToString(), AuditActionTypes.Delete, null, cancellationToken);
    }

    public async Task<BranchDto?> GetByIdAsync(int branchId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        return await _repository.GetByIdAsync(branchId, ResolveGymScope(gymId), cancellationToken);
    }

    public async Task<PagedResultDto<BranchDto>> GetPagedAsync(BranchSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = ResolveGymScope(query.GymId);
        await _repository.EnsureDefaultBranchAsync(gymId, cancellationToken);
        return await _repository.GetPagedAsync(gymId, query, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(Guid? gymId, bool includeInactive, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var scope = ResolveGymScope(gymId);
        await _repository.EnsureDefaultBranchAsync(scope, cancellationToken);
        return await _repository.GetAllAsync(scope, includeInactive, cancellationToken);
    }

    public async Task AssignManagerAsync(int branchId, AssignBranchManagerDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        await _repository.AssignManagerAsync(gymId, branchId, dto.UserId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchManager, branchId.ToString(), AuditActionTypes.Assign, dto, cancellationToken);
    }

    public async Task<int> TransferMemberAsync(TransferMemberBranchDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.TransferMembers))
            throw new UnauthorizedAccessException("Transfer members permission required.");
        var gymId = ResolveGymScope(null);
        var transferId = await _repository.TransferMemberAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchTransfer, transferId.ToString(), AuditActionTypes.Update, dto, cancellationToken);
        return transferId;
    }

    public async Task<int> TransferTrainerAsync(TransferTrainerBranchDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.TransferTrainers))
            throw new UnauthorizedAccessException("Transfer trainers permission required.");
        var gymId = ResolveGymScope(null);
        var transferId = await _repository.TransferTrainerAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchTransfer, transferId.ToString(), AuditActionTypes.Update, dto, cancellationToken);
        return transferId;
    }

    public async Task<PagedResultDto<BranchTransferDto>> GetTransferHistoryAsync(BranchTransferQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        return await _repository.GetTransferHistoryAsync(ResolveGymScope(query.GymId), query, cancellationToken);
    }

    public async Task<BranchTargetDto> UpsertTargetAsync(UpsertBranchTargetDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        var result = await _repository.UpsertTargetAsync(gymId, dto, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchTarget, result.TargetId.ToString(), AuditActionTypes.Update, result, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<BranchTargetDto>> GetTargetsAsync(int? branchId, DateOnly? targetMonth, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        return await _repository.GetTargetsAsync(ResolveGymScope(null), branchId, targetMonth, cancellationToken);
    }

    public async Task<BranchAnnouncementDto> CreateAnnouncementAsync(CreateBranchAnnouncementDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        var created = await _repository.CreateAnnouncementAsync(gymId, dto, _currentUser.UserId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchAnnouncement, created.AnnouncementId.ToString(), AuditActionTypes.Create, created, cancellationToken);
        if (dto.SendWhatsApp)
            await SendAnnouncementNotificationsAsync(gymId, created.AnnouncementId, created, cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<BranchAnnouncementDto>> GetAnnouncementsAsync(int? branchId, string? audience, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        return await _repository.GetAnnouncementsAsync(ResolveGymScope(null), branchId, audience, true, cancellationToken);
    }

    public async Task DeleteAnnouncementAsync(int announcementId, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = ResolveGymScope(null);
        await _repository.DeleteAnnouncementAsync(announcementId, gymId, cancellationToken);
        await LogAsync(gymId, AuditEntityNames.BranchAnnouncement, announcementId.ToString(), AuditActionTypes.Delete, null, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchDashboardItemDto>> GetDashboardAsync(int? branchId, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = ResolveGymScope(null);
        await _repository.EnsureDefaultBranchAsync(gymId, cancellationToken);
        return await _repository.GetDashboardAsync(gymId, branchId, cancellationToken);
    }

    public async Task<BranchAnalyticsDto> GetAnalyticsAsync(int months, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ViewBranchAnalytics))
            throw new UnauthorizedAccessException("View branch analytics permission required.");
        return await _repository.GetAnalyticsAsync(ResolveGymScope(null), months, cancellationToken);
    }

    private async Task SendAnnouncementNotificationsAsync(Guid gymId, int announcementId, BranchAnnouncementDto announcement, CancellationToken cancellationToken)
    {
        var recipients = await _repository.GetAnnouncementRecipientsAsync(gymId, announcementId, cancellationToken);
        foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r.Phone)))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = NotificationTypes.BranchAnnouncement,
                PhoneNumber = recipient.Phone!,
                RecipientUserId = recipient.RecipientUserId,
                Variables = new Dictionary<string, string>
                {
                    ["memberName"] = recipient.RecipientName ?? "Member",
                    ["title"] = announcement.Title,
                    ["message"] = announcement.Message
                },
                RelatedEntityType = AuditEntityNames.BranchAnnouncement,
                RelatedEntityId = announcementId.ToString()
            }, cancellationToken);
        }
    }

    private void EnsureCanView()
    {
        if (!_currentUser.HasPermission(Permissions.ViewBranches))
            throw new UnauthorizedAccessException("View branches permission required.");
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManageBranches))
            throw new UnauthorizedAccessException("Manage branches permission required.");
    }

    private Guid ResolveGymScope(Guid? requestedGymId) => GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private Guid ResolveGymIdForMutation(Guid? dtoGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
        {
            if (dtoGymId is null) throw new ArgumentException("GymId is required.");
            return dtoGymId.Value;
        }
        return _currentUser.RequireGymId();
    }

    private Task LogAsync(Guid gymId, string entity, string entityId, string action, object? value, CancellationToken cancellationToken) =>
        _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = entity,
            EntityId = entityId,
            ActionType = action,
            NewValue = value
        }, cancellationToken);
}
