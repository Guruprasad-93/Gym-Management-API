using System.Text.Json;
using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class NotificationService : INotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly INotificationRepository _repository;
    private readonly IWhatsAppProvider _whatsAppProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IWhiteLabelService _whiteLabelService;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IWhatsAppProvider whatsAppProvider,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IWhiteLabelService whiteLabelService,
        IOptions<WhatsAppSettings> settings,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _whatsAppProvider = whatsAppProvider;
        _currentUser = currentUser;
        _auditService = auditService;
        _whiteLabelService = whiteLabelService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<NotificationTemplateDto> CreateTemplateAsync(
        CreateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScope();
        var created = await _repository.CreateTemplateAsync(gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Notification,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task UpdateTemplateAsync(
        int templateId,
        UpdateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScope();
        await _repository.UpdateTemplateAsync(templateId, gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Notification,
            EntityId = templateId.ToString(),
            ActionType = AuditActionTypes.Update,
            NewValue = dto
        }, cancellationToken);
    }

    public async Task DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScope();
        await _repository.DeleteTemplateAsync(templateId, gymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Notification,
            EntityId = templateId.ToString(),
            ActionType = AuditActionTypes.Delete
        }, cancellationToken);
    }

    public Task<IReadOnlyList<NotificationTemplateDto>> GetTemplatesAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default) =>
        _repository.GetTemplatesAsync(ResolveGymScope(), includeInactive, cancellationToken);

    public Task<IReadOnlyList<NotificationSettingDto>> GetSettingsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetSettingsAsync(ResolveGymScope(), cancellationToken);

    public async Task UpdateSettingsAsync(
        IReadOnlyList<UpdateNotificationSettingDto> settings,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScope();
        foreach (var setting in settings)
            await _repository.UpsertSettingAsync(gymId, setting, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Notification,
            EntityId = "settings",
            ActionType = AuditActionTypes.Update,
            NewValue = settings
        }, cancellationToken);
    }

    public Task<NotificationDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        _repository.GetDashboardAsync(ResolveGymScope(), cancellationToken);

    public Task<PagedResultDto<NotificationLogDto>> SearchHistoryAsync(
        NotificationSearchQueryDto query,
        CancellationToken cancellationToken = default) =>
        _repository.SearchLogsAsync(ResolveGymScope(), query, cancellationToken);

    public async Task<NotificationLogDto> SendTestAsync(
        SendTestNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var gymId = ResolveGymScope();
        var templateName = dto.TemplateName ?? dto.NotificationType;
        var variables = dto.Variables ?? new Dictionary<string, string> { ["memberName"] = "Test User" };

        var request = new SendNotificationRequestDto
        {
            NotificationType = dto.NotificationType,
            PhoneNumber = dto.PhoneNumber,
            Variables = variables
        };

        await SendInternalAsync(gymId, request, templateName, null, sendImmediately: true, cancellationToken);

        var history = await _repository.SearchLogsAsync(gymId, new NotificationSearchQueryDto
        {
            Search = dto.PhoneNumber,
            PageNumber = 1,
            PageSize = 1
        }, cancellationToken);

        return history.Items.FirstOrDefault()
            ?? throw new InvalidOperationException("Test notification was not logged.");
    }

    public Task QueueAndSendAsync(SendNotificationRequestDto request, CancellationToken cancellationToken = default) =>
        SendInternalAsync(ResolveGymScope(), request, null, null, sendImmediately: true, cancellationToken);

    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetPendingAsync(_settings.PendingBatchSize, cancellationToken);
        foreach (var item in pending)
        {
            try
            {
                await DispatchPendingAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing notification log {LogId}", item.LogId);
            }
        }
    }

    public async Task QueueMembershipExpiryRemindersAsync(CancellationToken cancellationToken = default)
    {
        var gymIds = await _repository.GetActiveGymIdsAsync(cancellationToken);
        var mappings = new (int Days, string Type)[]
        {
            (7, NotificationTypes.MembershipExpiry7Days),
            (3, NotificationTypes.MembershipExpiry3Days),
            (0, NotificationTypes.MembershipExpiryToday)
        };

        foreach (var gymId in gymIds)
        {
            foreach (var (days, type) in mappings)
            {
                if (!await IsEnabledAsync(gymId, type, cancellationToken))
                    continue;

                var candidates = await _repository.GetExpiryCandidatesAsync(gymId, days, type, cancellationToken);
                foreach (var candidate in candidates)
                {
                    var variables = new Dictionary<string, string>
                    {
                        ["memberName"] = candidate.MemberName,
                        ["planName"] = candidate.PlanName,
                        ["expiryDate"] = candidate.EndDate.ToString("yyyy-MM-dd"),
                        ["daysRemaining"] = days.ToString()
                    };

                    await SendInternalAsync(
                        gymId,
                        new SendNotificationRequestDto
                        {
                            NotificationType = type,
                            PhoneNumber = candidate.MemberPhone,
                            RecipientUserId = candidate.RecipientUserId,
                            MemberId = candidate.MemberId,
                            Variables = variables,
                            RelatedEntityType = AuditEntityNames.Membership,
                            RelatedEntityId = candidate.MembershipId.ToString()
                        },
                        null,
                        null,
                        sendImmediately: false,
                        cancellationToken);
                }
            }
        }

        await ProcessPendingNotificationsAsync(cancellationToken);
    }

    public Task SendEventNotificationAsync(
        Guid gymId,
        SendNotificationRequestDto request,
        CancellationToken cancellationToken = default) =>
        TrySendEventAsync(
            gymId,
            request.NotificationType,
            request.PhoneNumber,
            request.RecipientUserId,
            request.MemberId,
            request.Variables,
            request.RelatedEntityType,
            request.RelatedEntityId,
            cancellationToken);

    internal async Task TrySendEventAsync(
        Guid gymId,
        string notificationType,
        string phoneNumber,
        Guid? recipientUserId,
        int? memberId,
        Dictionary<string, string> variables,
        string? relatedEntityType,
        string? relatedEntityId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsEnabledAsync(gymId, notificationType, cancellationToken))
                return;

            await SendInternalAsync(
                gymId,
                new SendNotificationRequestDto
                {
                    NotificationType = notificationType,
                    PhoneNumber = phoneNumber,
                    RecipientUserId = recipientUserId,
                    MemberId = memberId,
                    Variables = variables,
                    RelatedEntityType = relatedEntityType,
                    RelatedEntityId = relatedEntityId
                },
                null,
                null,
                sendImmediately: true,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Event notification {Type} failed for gym {GymId}", notificationType, gymId);
        }
    }

    private async Task SendInternalAsync(
        Guid gymId,
        SendNotificationRequestDto request,
        string? overrideTemplateName,
        int? overrideTemplateId,
        bool sendImmediately,
        CancellationToken cancellationToken)
    {
        request.Variables ??= new Dictionary<string, string>();
        await _whiteLabelService.EnrichNotificationVariablesAsync(gymId, request.Variables, cancellationToken);

        var templates = await _repository.GetTemplatesAsync(gymId, false, cancellationToken);
        var template = templates.FirstOrDefault(t => t.NotificationType == request.NotificationType);
        var settings = await _repository.GetSettingsAsync(gymId, cancellationToken);
        var setting = settings.FirstOrDefault(s => s.NotificationType == request.NotificationType);

        var templateName = overrideTemplateName
            ?? setting?.ProviderTemplateName
            ?? template?.TemplateName
            ?? request.NotificationType;

        var variablesJson = JsonSerializer.Serialize(request.Variables, JsonOptions);
        var logId = await _repository.LogNotificationAsync(new LogNotificationCommand
        {
            GymId = gymId,
            NotificationType = request.NotificationType,
            TemplateId = overrideTemplateId ?? template?.Id,
            RecipientPhone = request.PhoneNumber,
            RecipientUserId = request.RecipientUserId,
            MemberId = request.MemberId,
            WhatsAppTemplateName = templateName,
            VariablesJson = variablesJson,
            Status = NotificationStatuses.Pending,
            ScheduledFor = sendImmediately ? null : DateTime.UtcNow,
            RelatedEntityType = request.RelatedEntityType,
            RelatedEntityId = request.RelatedEntityId
        }, cancellationToken);

        if (sendImmediately)
        {
            await DispatchPendingAsync(new PendingNotificationRow
            {
                LogId = logId,
                GymId = gymId,
                NotificationType = request.NotificationType,
                TemplateId = template?.Id,
                RecipientPhone = request.PhoneNumber,
                WhatsAppTemplateName = templateName,
                VariablesJson = variablesJson
            }, cancellationToken);
        }
    }

    private async Task DispatchPendingAsync(PendingNotificationRow item, CancellationToken cancellationToken)
    {
        var variables = string.IsNullOrWhiteSpace(item.VariablesJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(item.VariablesJson, JsonOptions)
              ?? new Dictionary<string, string>();

        var result = await _whatsAppProvider.SendTemplateAsync(new WhatsAppTemplateMessage
        {
            PhoneNumber = item.RecipientPhone,
            TemplateName = item.WhatsAppTemplateName,
            Variables = variables
        }, cancellationToken);

        var status = result.Success ? NotificationStatuses.Sent : NotificationStatuses.Failed;
        await _repository.UpdateLogStatusAsync(
            item.LogId,
            item.GymId,
            status,
            result.ErrorMessage,
            result.ProviderMessageId,
            result.Success ? DateTime.UtcNow : null,
            cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = item.GymId,
            EntityName = AuditEntityNames.Notification,
            EntityId = item.LogId.ToString(),
            ActionType = result.Success ? AuditActionTypes.Send : AuditActionTypes.Update,
            NewValue = new { item.NotificationType, item.RecipientPhone, status, result.ErrorMessage, failed = !result.Success }
        }, cancellationToken);
    }

    private async Task<bool> IsEnabledAsync(Guid gymId, string notificationType, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetSettingsAsync(gymId, cancellationToken);
        var setting = settings.FirstOrDefault(s => s.NotificationType == notificationType);
        return setting?.IsEnabled ?? true;
    }

    private Guid ResolveGymScope() => GymScopeResolver.ResolveRequired(_currentUser);
}
