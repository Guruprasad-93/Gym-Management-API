using System.Data;
using Dapper;
using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class SubscriptionNotificationRepository : ISubscriptionNotificationRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public SubscriptionNotificationRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<bool> CreateNotificationAsync(
        Guid gymId,
        Guid userId,
        string notificationKey,
        string notificationType,
        string title,
        string message,
        string severity,
        string? actionRoute,
        bool showLoginPopup,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@NotificationKey", notificationKey);
        parameters.Add("@NotificationType", notificationType);
        parameters.Add("@Title", title);
        parameters.Add("@Message", message);
        parameters.Add("@Severity", severity);
        parameters.Add("@ActionRoute", actionRoute);
        parameters.Add("@ShowLoginPopup", showLoginPopup);
        parameters.Add("@Created", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.UserInAppNotificationCreate, parameters, cancellationToken);
        return parameters.Get<bool>("@Created");
    }

    public async Task<IReadOnlyList<UserInAppNotificationDto>> GetForUserAsync(
        Guid gymId,
        Guid userId,
        bool unreadOnly,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<UserInAppNotificationRow>(
            StoredProcedureNames.UserInAppNotificationsGetForUser,
            new { GymId = gymId, UserId = userId, UnreadOnly = unreadOnly, Top = 50 },
            cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid gymId, Guid userId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@UnreadCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.UserInAppNotificationsGetUnreadCount, parameters, cancellationToken);
        return parameters.Get<int>("@UnreadCount");
    }

    public Task MarkReadAsync(
        Guid gymId,
        Guid userId,
        IReadOnlyList<int>? notificationIds,
        CancellationToken cancellationToken = default)
    {
        var ids = notificationIds is null || notificationIds.Count == 0
            ? null
            : string.Join(',', notificationIds);
        return _sp.ExecuteAsync(
            StoredProcedureNames.UserInAppNotificationsMarkRead,
            new { GymId = gymId, UserId = userId, NotificationIds = ids },
            cancellationToken);
    }

    public async Task<IReadOnlyList<GymSubscriptionDto>> GetActiveSubscriptionsForNotificationJobAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymSubscriptionRow>(
            StoredProcedureNames.SubscriptionExpiryGetActiveSubscriptions,
            cancellationToken: cancellationToken);
        return rows.Select(MapSubscription).ToList();
    }

    public async Task<IReadOnlyList<SubscriptionTenantUserDto>> GetGymTenantUsersAsync(
        Guid gymId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<SubscriptionTenantUserRow>(
            StoredProcedureNames.SubscriptionExpiryGetGymTenantUsers,
            new { GymId = gymId },
            cancellationToken);
        return rows.Select(r => new SubscriptionTenantUserDto
        {
            UserId = r.UserId,
            RoleName = r.RoleName
        }).ToList();
    }

    private static UserInAppNotificationDto Map(UserInAppNotificationRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        UserId = row.UserId,
        NotificationKey = row.NotificationKey,
        NotificationType = row.NotificationType,
        Title = row.Title,
        Message = row.Message,
        Severity = row.Severity,
        ActionRoute = row.ActionRoute,
        ShowLoginPopup = row.ShowLoginPopup,
        IsRead = row.IsRead,
        ReadDate = row.ReadDate,
        CreatedDate = row.CreatedDate
    };

    private static GymSubscriptionDto MapSubscription(GymSubscriptionRow row) => new()
    {
        Id = row.GymSubscriptionId,
        GymId = row.GymId,
        SaasPlanId = row.SaasPlanId,
        PlanCode = row.PlanCode,
        PlanName = row.PlanName,
        Status = row.Status,
        BillingCycle = row.BillingCycle,
        Amount = row.Amount,
        StartDate = row.StartDate,
        EndDate = row.EndDate,
        TrialEndsAt = row.TrialEndsAt,
        CurrentPeriodEnd = row.CurrentPeriodEnd,
        GraceEndsAt = row.GraceEndsAt,
        HasAccess = row.HasAccess,
        CancelAtPeriodEnd = row.CancelAtPeriodEnd,
        MaxMembers = row.MaxMembers,
        MaxTrainers = row.MaxTrainers,
        StorageLimitMb = row.StorageLimitMb,
        WhatsAppNotificationLimit = row.WhatsAppNotificationLimit,
        MonthlyPrice = row.MonthlyPrice,
        YearlyPrice = row.YearlyPrice
    };

    private sealed class UserInAppNotificationRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public Guid UserId { get; set; }
        public string NotificationKey { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? ActionRoute { get; set; }
        public bool ShowLoginPopup { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private sealed class SubscriptionTenantUserRow
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
