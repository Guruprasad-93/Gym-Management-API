using Gym.Application.Authorization;
using Gym.Application.Common;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Leads;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Gym.Domain.Entities;

namespace Gym.Application.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IGymRepository _gymRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ILeadReportExporter _exporter;
    private readonly ITenantLimitService _tenantLimits;

    public LeadService(
        ILeadRepository leadRepository,
        IMemberRepository memberRepository,
        IMembershipRepository membershipRepository,
        IUserRepository userRepository,
        ITrainerRepository trainerRepository,
        IGymRepository gymRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        IAuditService auditService,
        INotificationService notificationService,
        ILeadReportExporter exporter,
        ITenantLimitService tenantLimits)
    {
        _leadRepository = leadRepository;
        _memberRepository = memberRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _trainerRepository = trainerRepository;
        _gymRepository = gymRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _auditService = auditService;
        _notificationService = notificationService;
        _exporter = exporter;
        _tenantLimits = tenantLimits;
    }

    public async Task<LeadDto> CreateAsync(CreateLeadDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var gymId = ResolveGymIdForMutation(dto.GymId);
        ValidateLeadSource(dto.LeadSource);
        var created = await _leadRepository.CreateAsync(gymId, _currentUser.UserId, dto, cancellationToken);

        await _leadRepository.CreateActivityAsync(created.Id, gymId, LeadActivityTypes.Created,
            $"Lead created from {dto.LeadSource}.", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);

        await SendLeadNotificationAsync(gymId, NotificationTypes.LeadCreated, created, new Dictionary<string, string>
        {
            ["leadName"] = created.FullName,
            ["leadSource"] = created.LeadSource,
            ["mobileNumber"] = created.MobileNumber
        }, cancellationToken);

        return created;
    }

    public async Task<LeadDto> UpdateAsync(int id, UpdateLeadDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var existing = await GetLeadWithAccessCheckAsync(id, dto.GymId, cancellationToken);
        var gymId = existing.GymId;
        ValidateLeadSource(dto.LeadSource);
        ValidateLeadStatus(dto.Status);

        var oldStatus = existing.Status;
        await _leadRepository.UpdateAsync(id, gymId, dto, cancellationToken);
        var updated = (await _leadRepository.GetByIdAsync(id, gymId, null, cancellationToken))!;

        await _leadRepository.CreateActivityAsync(id, gymId, LeadActivityTypes.Updated,
            $"Lead details updated.", _currentUser.UserId, cancellationToken);

        if (!string.Equals(oldStatus, updated.Status, StringComparison.Ordinal))
        {
            var activityType = updated.Status == LeadStatuses.Lost ? LeadActivityTypes.MarkedLost : LeadActivityTypes.StatusChanged;
            await _leadRepository.CreateActivityAsync(id, gymId, activityType,
                $"Status changed from {oldStatus} to {updated.Status}.", _currentUser.UserId, cancellationToken);
        }

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = SnapshotLead(existing),
            NewValue = SnapshotLead(updated)
        }, cancellationToken);

        return updated;
    }

    public async Task<LeadDto> UpdateStatusAsync(int id, UpdateLeadStatusDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var existing = await GetLeadWithAccessCheckAsync(id, dto.GymId, cancellationToken);
        ValidateLeadStatus(dto.Status);

        await _leadRepository.UpdateStatusAsync(id, existing.GymId, dto.Status, cancellationToken);
        var updated = (await _leadRepository.GetByIdAsync(id, existing.GymId, null, cancellationToken))!;

        var activityType = dto.Status == LeadStatuses.Lost ? LeadActivityTypes.MarkedLost : LeadActivityTypes.StatusChanged;
        await _leadRepository.CreateActivityAsync(id, existing.GymId, activityType,
            $"Status changed to {dto.Status}.", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = new { existing.Status },
            NewValue = new { Status = dto.Status }
        }, cancellationToken);

        return updated;
    }

    public async Task DeleteAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var existing = await GetLeadWithAccessCheckAsync(id, gymId, cancellationToken);
        await _leadRepository.DeleteAsync(id, existing.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = SnapshotLead(existing)
        }, cancellationToken);
    }

    public async Task<LeadDetailDto> GetDetailAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadWithAccessCheckAsync(id, gymId, cancellationToken);
        var scope = lead.GymId;
        var activities = await _leadRepository.GetActivitiesAsync(id, scope, cancellationToken);
        var followUps = await _leadRepository.GetFollowUpsAsync(id, scope, cancellationToken);
        var trials = await _leadRepository.GetTrialsAsync(id, scope, cancellationToken);
        return new LeadDetailDto { Lead = lead, Activities = activities, FollowUps = followUps, Trials = trials };
    }

    public async Task<PagedResultDto<LeadDto>> GetPagedAsync(LeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScopeForQuery(query.GymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _leadRepository.GetPagedAsync(gymId, trainerFilter, query, cancellationToken);
    }

    public async Task<LeadDto> AssignTrainerAsync(AssignTrainerToLeadDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var lead = await GetLeadWithAccessCheckAsync(dto.LeadId, dto.GymId, cancellationToken);
        await _leadRepository.AssignTrainerAsync(dto.LeadId, lead.GymId, dto.TrainerId, cancellationToken);
        var updated = (await _leadRepository.GetByIdAsync(dto.LeadId, lead.GymId, null, cancellationToken))!;

        await _leadRepository.CreateActivityAsync(dto.LeadId, lead.GymId, LeadActivityTypes.TrainerAssigned,
            $"Trainer assigned: {updated.AssignedTrainerName ?? dto.TrainerId.ToString()}.", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = lead.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = dto.LeadId.ToString(),
            ActionType = AuditActionTypes.Assign,
            NewValue = new { dto.TrainerId, updated.AssignedTrainerName }
        }, cancellationToken);

        return updated;
    }

    public async Task<LeadTrialDto> ScheduleTrialAsync(ScheduleTrialDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var lead = await GetLeadWithAccessCheckAsync(dto.LeadId, dto.GymId, cancellationToken);
        var trialId = await _leadRepository.ScheduleTrialAsync(
            dto.LeadId, lead.GymId, dto.TrainerId, dto.TrialDate, _currentUser.UserId, cancellationToken);

        await _leadRepository.CreateActivityAsync(dto.LeadId, lead.GymId, LeadActivityTypes.TrialScheduled,
            $"Trial scheduled for {dto.TrialDate:yyyy-MM-dd HH:mm}.", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = lead.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = dto.LeadId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = new { TrialId = trialId, dto.TrialDate, dto.TrainerId }
        }, cancellationToken);

        await SendLeadNotificationAsync(lead.GymId, NotificationTypes.TrialScheduled, lead, new Dictionary<string, string>
        {
            ["leadName"] = lead.FullName,
            ["trialDate"] = dto.TrialDate.ToString("yyyy-MM-dd HH:mm")
        }, cancellationToken);

        var trials = await _leadRepository.GetTrialsAsync(dto.LeadId, lead.GymId, cancellationToken);
        return trials.First(t => t.Id == trialId);
    }

    public async Task RecordTrialFeedbackAsync(RecordTrialFeedbackDto dto, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadWithAccessCheckAsync(dto.LeadId, dto.GymId, cancellationToken);
        await _leadRepository.RecordTrialFeedbackAsync(dto.TrialId, lead.GymId, dto, cancellationToken);

        await _leadRepository.CreateActivityAsync(dto.LeadId, lead.GymId, LeadActivityTypes.TrialCompleted,
            $"Trial feedback recorded ({dto.AttendanceStatus}).", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = lead.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = dto.LeadId.ToString(),
            ActionType = AuditActionTypes.Update,
            NewValue = dto
        }, cancellationToken);
    }

    public async Task<LeadFollowUpDto> CreateFollowUpAsync(CreateLeadFollowUpDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var lead = await GetLeadWithAccessCheckAsync(dto.LeadId, dto.GymId, cancellationToken);
        var followUpId = await _leadRepository.CreateFollowUpAsync(dto.LeadId, lead.GymId, _currentUser.UserId, dto, cancellationToken);

        await _leadRepository.CreateActivityAsync(dto.LeadId, lead.GymId, LeadActivityTypes.FollowUpCreated,
            $"Follow-up scheduled ({dto.FollowUpType}) for {dto.FollowUpDate:yyyy-MM-dd HH:mm}.", _currentUser.UserId, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = lead.GymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = dto.LeadId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = dto
        }, cancellationToken);

        var followUps = await _leadRepository.GetFollowUpsAsync(dto.LeadId, lead.GymId, cancellationToken);
        return followUps.First(f => f.Id == followUpId);
    }

    public async Task<LeadFollowUpDto> UpdateFollowUpAsync(
        int followUpId, UpdateLeadFollowUpDto dto, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanManageLeads();
        var scope = ResolveGymScopeForQuery(gymId);
        await _leadRepository.UpdateFollowUpAsync(followUpId, scope, dto, cancellationToken);
        var allFollowUps = await _leadRepository.GetPendingFollowUpsAsync(scope, null, 500, cancellationToken);
        var match = allFollowUps.FirstOrDefault(f => f.Id == followUpId);
        if (match is not null)
            return match;
        // Completed follow-ups fall out of pending list; return synthetic dto
        return new LeadFollowUpDto
        {
            Id = followUpId,
            FollowUpDate = dto.FollowUpDate,
            FollowUpType = dto.FollowUpType,
            Remarks = dto.Remarks,
            Status = dto.Status,
            NextFollowUpDate = dto.NextFollowUpDate
        };
    }

    public async Task<ConvertLeadResultDto> ConvertToMemberAsync(ConvertLeadToMemberDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ConvertLeads))
            throw new UnauthorizedAccessException("You do not have permission to convert leads.");

        var lead = await GetLeadWithAccessCheckAsync(dto.LeadId, dto.GymId, cancellationToken);
        if (lead.Status == LeadStatuses.Converted)
            throw new InvalidOperationException("Lead is already converted.");
        if (lead.ConvertedMemberId.HasValue)
            throw new InvalidOperationException("Lead is already linked to a member.");

        var gymId = lead.GymId;
        await _tenantLimits.EnsureCanAddMemberAsync(gymId, cancellationToken);

        var email = string.IsNullOrWhiteSpace(dto.Email ?? lead.Email)
            ? null
            : (dto.Email ?? lead.Email)!.Trim().ToLowerInvariant();

        var loginIdentifier = !string.IsNullOrWhiteSpace(dto.LoginIdentifier)
            ? Validation.LoginIdentifierRules.Normalize(dto.LoginIdentifier)
            : $"MEM{lead.Id:D6}";

        if (string.IsNullOrWhiteSpace(loginIdentifier))
            loginIdentifier = $"MEM{lead.Id:D6}";

        Validation.LoginIdentifierRules.Validate(loginIdentifier);

        if (await _userRepository.ExistsByLoginIdentifierAsync(loginIdentifier, cancellationToken))
            throw new InvalidOperationException("A user with this login identifier already exists.");

        if (!string.IsNullOrWhiteSpace(email) && await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("A user with this email already exists.");

        string? temporaryPassword = null;
        var plainPassword = string.IsNullOrWhiteSpace(dto.Password)
            ? TemporaryPasswordGenerator.Generate()
            : dto.Password!;
        if (string.IsNullOrWhiteSpace(dto.Password))
            temporaryPassword = plainPassword;

        var user = User.Create(lead.FullName.Trim(), loginIdentifier, _passwordHasher.Hash(plainPassword), gymId, email);
        await _userRepository.AddAsync(user, cancellationToken);

        var member = await _memberRepository.CreateAsync(gymId, user.Id, new CreateMemberDto
        {
            GymId = gymId,
            Name = lead.FullName.Trim(),
            LoginIdentifier = loginIdentifier,
            Email = email,
            Password = plainPassword,
            Phone = lead.MobileNumber,
            Gender = lead.Gender,
            Address = lead.Address,
            TrainerId = lead.AssignedTrainerId,
            JoinDate = dto.StartDate
        }, cancellationToken);

        var membership = await _membershipRepository.CreateAsync(gymId, new CreateMembershipDto
        {
            MemberId = member.Id,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            Notes = $"Converted from lead #{lead.Id}"
        }, cancellationToken);

        await _leadRepository.MarkConvertedAsync(dto.LeadId, gymId, member.Id, cancellationToken);
        await _leadRepository.CreateActivityAsync(dto.LeadId, gymId, LeadActivityTypes.Converted,
            $"Converted to member #{member.Id} with membership #{membership.Id}.", _currentUser.UserId, cancellationToken);

        var updatedLead = (await _leadRepository.GetByIdAsync(dto.LeadId, gymId, null, cancellationToken))!;

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = dto.LeadId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = new { MemberId = member.Id, MembershipId = membership.Id }
        }, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Member,
            EntityId = member.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = member
        }, cancellationToken);

        await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
        {
            NotificationType = NotificationTypes.NewMemberRegistration,
            PhoneNumber = lead.MobileNumber,
            RecipientUserId = user.Id,
            MemberId = member.Id,
            Variables = new Dictionary<string, string>
            {
                ["memberName"] = member.FullName,
                ["email"] = email
            },
            RelatedEntityType = AuditEntityNames.Member,
            RelatedEntityId = member.Id.ToString()
        }, cancellationToken);

        await SendLeadNotificationAsync(gymId, NotificationTypes.LeadConverted, updatedLead, new Dictionary<string, string>
        {
            ["leadName"] = lead.FullName,
            ["memberName"] = member.FullName
        }, cancellationToken);

        return new ConvertLeadResultDto
        {
            Lead = updatedLead,
            MemberId = member.Id,
            MembershipId = membership.Id,
            TemporaryPassword = temporaryPassword
        };
    }

    public Task<LeadDashboardDto> GetDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _leadRepository.GetDashboardAsync(ResolveAnalyticsScope(gymId), cancellationToken);

    public async Task<LeadAnalyticsDto> GetAnalyticsAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ViewLeadAnalytics))
            throw new UnauthorizedAccessException("You do not have permission to view lead analytics.");

        var scope = ResolveAnalyticsScope(gymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return new LeadAnalyticsDto
        {
            Dashboard = await _leadRepository.GetDashboardAsync(scope, cancellationToken),
            LeadsBySource = await _leadRepository.GetSourceAnalyticsAsync(scope, cancellationToken),
            LeadsByStatus = await _leadRepository.GetStatusAnalyticsAsync(scope, cancellationToken),
            MonthlyConversions = await _leadRepository.GetConversionReportAsync(scope, 12, cancellationToken),
            TrainerPerformance = await _leadRepository.GetTrainerPerformanceAsync(scope, cancellationToken),
            PendingFollowUps = await _leadRepository.GetPendingFollowUpsAsync(scope, trainerFilter, 20, cancellationToken),
            TodaysTrials = await _leadRepository.GetTodaysTrialsAsync(scope, trainerFilter, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<LeadFollowUpDto>> GetPendingFollowUpsAsync(
        Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScopeForQuery(gymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _leadRepository.GetPendingFollowUpsAsync(scope, trainerFilter, 50, cancellationToken);
    }

    public async Task<IReadOnlyList<LeadTrialDto>> GetTodaysTrialsAsync(
        Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScopeForQuery(gymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _leadRepository.GetTodaysTrialsAsync(scope, trainerFilter, cancellationToken);
    }

    public async Task<byte[]> ExportPdfAsync(string reportType, LeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScopeForQuery(query.GymId);
        var gymName = await GetGymNameAsync(scope, cancellationToken);
        byte[] bytes;

        if (string.Equals(reportType, "conversion", StringComparison.OrdinalIgnoreCase))
        {
            var analytics = await BuildAnalyticsForExportAsync(scope, cancellationToken);
            bytes = _exporter.ExportConversionReportPdf(analytics, gymName);
        }
        else
        {
            var leads = await _leadRepository.SearchAllAsync(scope, await ResolveTrainerFilterAsync(cancellationToken), query, cancellationToken);
            bytes = _exporter.ExportLeadSummaryPdf(leads, gymName);
        }

        await LogExportAsync(scope, reportType, "pdf", cancellationToken);
        return bytes;
    }

    public async Task<byte[]> ExportExcelAsync(string reportType, LeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScopeForQuery(query.GymId);
        var gymName = await GetGymNameAsync(scope, cancellationToken);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        byte[] bytes;

        if (string.Equals(reportType, "conversion", StringComparison.OrdinalIgnoreCase))
        {
            var analytics = await BuildAnalyticsForExportAsync(scope, cancellationToken);
            bytes = _exporter.ExportConversionReportExcel(analytics, gymName);
        }
        else if (string.Equals(reportType, "followups", StringComparison.OrdinalIgnoreCase))
        {
            var followUps = await _leadRepository.GetPendingFollowUpsAsync(scope, trainerFilter, 5000, cancellationToken);
            bytes = _exporter.ExportFollowUpReportExcel(followUps, gymName);
        }
        else
        {
            var leads = await _leadRepository.SearchAllAsync(scope, trainerFilter, query, cancellationToken);
            bytes = _exporter.ExportLeadSummaryExcel(leads, gymName);
        }

        await LogExportAsync(scope, reportType, "excel", cancellationToken);
        return bytes;
    }

    public async Task ProcessRemindersAsync(CancellationToken cancellationToken = default)
    {
        var candidates = await _leadRepository.GetReminderCandidatesAsync(24, cancellationToken);
        foreach (var item in candidates)
        {
            var notificationType = item.ReminderType switch
            {
                "TrialReminder" => NotificationTypes.TrialReminder,
                "FollowUpReminder" => NotificationTypes.FollowUpReminder,
                _ => null
            };
            if (notificationType is null)
                continue;

            await _notificationService.SendEventNotificationAsync(item.GymId, new SendNotificationRequestDto
            {
                NotificationType = notificationType,
                PhoneNumber = item.MobileNumber,
                Variables = new Dictionary<string, string>
                {
                    ["leadName"] = item.LeadName,
                    ["scheduledAt"] = item.ScheduledAt.ToString("yyyy-MM-dd HH:mm")
                },
                RelatedEntityType = AuditEntityNames.Lead,
                RelatedEntityId = item.LeadId.ToString()
            }, cancellationToken);
        }
    }

    private async Task<LeadAnalyticsDto> BuildAnalyticsForExportAsync(Guid? scope, CancellationToken cancellationToken) =>
        new()
        {
            Dashboard = await _leadRepository.GetDashboardAsync(scope, cancellationToken),
            LeadsBySource = await _leadRepository.GetSourceAnalyticsAsync(scope, cancellationToken),
            LeadsByStatus = await _leadRepository.GetStatusAnalyticsAsync(scope, cancellationToken),
            MonthlyConversions = await _leadRepository.GetConversionReportAsync(scope, 12, cancellationToken),
            TrainerPerformance = await _leadRepository.GetTrainerPerformanceAsync(scope, cancellationToken),
            PendingFollowUps = await _leadRepository.GetPendingFollowUpsAsync(scope, null, 100, cancellationToken),
            TodaysTrials = await _leadRepository.GetTodaysTrialsAsync(scope, null, cancellationToken)
        };

    private async Task<LeadDto> GetLeadWithAccessCheckAsync(int leadId, Guid? requestedGymId, CancellationToken cancellationToken)
    {
        var gymScope = ResolveGymScopeForQuery(requestedGymId);
        var trainerFilter = await ResolveTrainerFilterAsync(cancellationToken);
        return await _leadRepository.GetByIdAsync(leadId, gymScope, trainerFilter, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");
    }

    private async Task<int?> ResolveTrainerFilterAsync(CancellationToken cancellationToken)
    {
        if (!IsTrainerOnly())
            return null;
        return await GetCurrentTrainerIdAsync(cancellationToken);
    }

    private async Task<int> GetCurrentTrainerIdAsync(CancellationToken cancellationToken)
    {
        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
        return trainer.Id;
    }

    private void EnsureCanManageLeads()
    {
        if (!_currentUser.HasPermission(Permissions.ManageLeads))
            throw new UnauthorizedAccessException("You do not have permission to manage leads.");
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole(RoleNames.Trainer)
        && !_currentUser.HasRole(RoleNames.GymAdmin)
        && !_currentUser.HasRole(RoleNames.SuperAdmin);

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

    private Guid ResolveGymScopeForQuery(Guid? requestedGymId) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private Guid? ResolveAnalyticsScope(Guid? requestedGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return requestedGymId;
        return GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
    }

    private async Task<string> GetGymNameAsync(Guid gymId, CancellationToken cancellationToken)
    {
        var gym = await _gymRepository.GetByIdAsync(gymId, cancellationToken);
        return gym?.Name ?? "Gym";
    }

    private Task SendLeadNotificationAsync(
        Guid gymId, string notificationType, LeadDto lead,
        Dictionary<string, string> variables, CancellationToken cancellationToken) =>
        _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
        {
            NotificationType = notificationType,
            PhoneNumber = lead.MobileNumber,
            Variables = variables,
            RelatedEntityType = AuditEntityNames.Lead,
            RelatedEntityId = lead.Id.ToString()
        }, cancellationToken);

    private static void ValidateLeadSource(string source)
    {
        if (!LeadSources.All.Contains(source))
            throw new ArgumentException($"Invalid lead source: {source}");
    }

    private static void ValidateLeadStatus(string status)
    {
        if (!LeadStatuses.All.Contains(status))
            throw new ArgumentException($"Invalid lead status: {status}");
    }

    private static object SnapshotLead(LeadDto l) => new
    {
        l.Id,
        l.FullName,
        l.MobileNumber,
        l.Email,
        l.LeadSource,
        l.Status,
        l.AssignedTrainerId
    };

    private Task LogExportAsync(Guid gymId, string reportType, string format, CancellationToken cancellationToken) =>
        _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Lead,
            EntityId = reportType,
            ActionType = AuditActionTypes.Export,
            NewValue = new { reportType, format }
        }, cancellationToken);
}
