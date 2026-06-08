using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public NotificationRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<NotificationTemplateDto> CreateTemplateAsync(
        Guid gymId,
        CreateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@NotificationType", dto.NotificationType);
        parameters.Add("@TemplateName", dto.TemplateName);
        parameters.Add("@BodyTemplate", dto.BodyTemplate);
        parameters.Add("@VariablesJson", dto.VariablesJson);
        parameters.Add("@IsActive", dto.IsActive);
        parameters.Add("@TemplateId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var templateId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateNotificationTemplate, parameters, "@TemplateId", cancellationToken);

        return (await GetTemplatesAsync(gymId, true, cancellationToken)).First(t => t.Id == templateId);
    }

    public async Task UpdateTemplateAsync(
        int templateId,
        Guid gymId,
        UpdateNotificationTemplateDto dto,
        CancellationToken cancellationToken = default) =>
        await _sp.ExecuteAsync(
            StoredProcedureNames.UpdateNotificationTemplate,
            new
            {
                TemplateId = templateId,
                GymId = gymId,
                dto.NotificationType,
                dto.TemplateName,
                dto.BodyTemplate,
                dto.VariablesJson,
                dto.IsActive
            },
            cancellationToken);

    public async Task DeleteTemplateAsync(int templateId, Guid gymId, CancellationToken cancellationToken = default) =>
        await _sp.ExecuteAsync(
            StoredProcedureNames.DeleteNotificationTemplate,
            new { TemplateId = templateId, GymId = gymId },
            cancellationToken);

    public async Task<IReadOnlyList<NotificationTemplateDto>> GetTemplatesAsync(
        Guid gymId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<NotificationTemplateRow>(
            StoredProcedureNames.GetNotificationTemplates,
            new { GymId = gymId, IncludeInactive = includeInactive },
            cancellationToken);

        return rows.Select(MapTemplate).ToList();
    }

    public async Task UpsertSettingAsync(
        Guid gymId,
        UpdateNotificationSettingDto dto,
        CancellationToken cancellationToken = default) =>
        await _sp.ExecuteAsync(
            StoredProcedureNames.UpsertNotificationSetting,
            new
            {
                GymId = gymId,
                dto.NotificationType,
                dto.IsEnabled,
                dto.ProviderTemplateName
            },
            cancellationToken);

    public async Task<IReadOnlyList<NotificationSettingDto>> GetSettingsAsync(
        Guid gymId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<NotificationSettingRow>(
            StoredProcedureNames.GetNotificationSettings,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(r => new NotificationSettingDto
        {
            Id = r.SettingId,
            GymId = r.GymId,
            NotificationType = r.NotificationType,
            IsEnabled = r.IsEnabled,
            ProviderTemplateName = r.ProviderTemplateName
        }).ToList();
    }

    public async Task<long> LogNotificationAsync(
        LogNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", command.GymId);
        parameters.Add("@NotificationType", command.NotificationType);
        parameters.Add("@TemplateId", command.TemplateId);
        parameters.Add("@RecipientPhone", command.RecipientPhone);
        parameters.Add("@RecipientUserId", command.RecipientUserId);
        parameters.Add("@MemberId", command.MemberId);
        parameters.Add("@WhatsAppTemplateName", command.WhatsAppTemplateName);
        parameters.Add("@VariablesJson", command.VariablesJson);
        parameters.Add("@Status", command.Status);
        parameters.Add("@ErrorMessage", command.ErrorMessage);
        parameters.Add("@ProviderMessageId", command.ProviderMessageId);
        parameters.Add("@ScheduledFor", command.ScheduledFor);
        parameters.Add("@SentAt", command.SentAt);
        parameters.Add("@RelatedEntityType", command.RelatedEntityType);
        parameters.Add("@RelatedEntityId", command.RelatedEntityId);
        parameters.Add("@LogId", dbType: DbType.Int64, direction: ParameterDirection.Output);

        return await _sp.ExecuteWithOutputAsync<long>(
            StoredProcedureNames.LogNotification, parameters, "@LogId", cancellationToken);
    }

    public async Task UpdateLogStatusAsync(
        long logId,
        Guid gymId,
        string status,
        string? errorMessage,
        string? providerMessageId,
        DateTime? sentAt,
        CancellationToken cancellationToken = default) =>
        await _sp.ExecuteAsync(
            StoredProcedureNames.UpdateNotificationLogStatus,
            new { LogId = logId, GymId = gymId, Status = status, ErrorMessage = errorMessage, ProviderMessageId = providerMessageId, SentAt = sentAt },
            cancellationToken);

    public async Task<PagedResultDto<NotificationLogDto>> SearchLogsAsync(
        Guid gymId,
        NotificationSearchQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@NotificationType", query.NotificationType);
        parameters.Add("@Status", query.Status);
        parameters.Add("@FromDate", query.FromDate);
        parameters.Add("@ToDate", query.ToDate);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<NotificationLogRow>(
            StoredProcedureNames.SearchNotificationLogs,
            parameters,
            cancellationToken);

        return new PagedResultDto<NotificationLogDto>
        {
            Items = rows.Select(MapLog).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<NotificationDashboardDto> GetDashboardAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<NotificationDashboardRow>(
            StoredProcedureNames.GetNotificationDashboard,
            new { GymId = gymId },
            cancellationToken);

        return new NotificationDashboardDto
        {
            TotalLogs = row?.TotalLogs ?? 0,
            SentCount = row?.SentCount ?? 0,
            FailedCount = row?.FailedCount ?? 0,
            PendingCount = row?.PendingCount ?? 0,
            ActiveTemplates = row?.ActiveTemplates ?? 0,
            SentToday = row?.SentToday ?? 0
        };
    }

    public async Task<IReadOnlyList<PendingNotificationRow>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PendingNotificationRow>(
            StoredProcedureNames.GetPendingNotifications,
            new { BatchSize = batchSize },
            cancellationToken);

        return rows.ToList();
    }

    public async Task<IReadOnlyList<ExpiryNotificationCandidateRow>> GetExpiryCandidatesAsync(
        Guid gymId,
        int daysBeforeExpiry,
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<ExpiryNotificationCandidateRow>(
            StoredProcedureNames.GetMembershipsExpiringForNotification,
            new { GymId = gymId, DaysBeforeExpiry = daysBeforeExpiry, NotificationType = notificationType },
            cancellationToken);

        return rows.ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetActiveGymIdsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymIdRow>(
            StoredProcedureNames.GetAllActiveGymIds,
            cancellationToken: cancellationToken);

        return rows.Select(r => r.GymId).ToList();
    }

    private static NotificationTemplateDto MapTemplate(NotificationTemplateRow row) => new()
    {
        Id = row.TemplateId,
        GymId = row.GymId,
        NotificationType = row.NotificationType,
        TemplateName = row.TemplateName,
        BodyTemplate = row.BodyTemplate,
        VariablesJson = row.VariablesJson,
        IsActive = row.IsActive,
        CreatedAt = row.CreatedAt,
        UpdatedAt = row.UpdatedAt
    };

    private static NotificationLogDto MapLog(NotificationLogRow row) => new()
    {
        Id = row.LogId,
        GymId = row.GymId,
        NotificationType = row.NotificationType,
        TemplateId = row.TemplateId,
        RecipientPhone = row.RecipientPhone,
        RecipientName = row.RecipientName,
        MemberId = row.MemberId,
        WhatsAppTemplateName = row.WhatsAppTemplateName,
        VariablesJson = row.VariablesJson,
        Status = row.Status,
        ErrorMessage = row.ErrorMessage,
        ProviderMessageId = row.ProviderMessageId,
        ScheduledFor = row.ScheduledFor,
        SentAt = row.SentAt,
        CreatedAt = row.CreatedAt,
        RelatedEntityType = row.RelatedEntityType,
        RelatedEntityId = row.RelatedEntityId
    };
}

internal sealed class GymIdRow
{
    public Guid GymId { get; set; }
}
