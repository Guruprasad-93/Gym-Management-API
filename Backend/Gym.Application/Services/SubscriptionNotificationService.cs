using Gym.Application.Constants;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Application.Services;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class SubscriptionNotificationService : ISubscriptionNotificationService
{
    private const string GymAdminRenewRoute = "/gym-admin/renew-subscription";

    private readonly ISubscriptionNotificationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public SubscriptionNotificationService(
        ISubscriptionNotificationRepository repository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<UserInAppNotificationsResponseDto> GetMyNotificationsAsync(
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User context is required.");

        var items = await _repository.GetForUserAsync(gymId, userId, unreadOnly, cancellationToken);
        var unreadCount = await _repository.GetUnreadCountAsync(gymId, userId, cancellationToken);
        return new UserInAppNotificationsResponseDto
        {
            Items = items,
            UnreadCount = unreadCount
        };
    }

    public Task MarkReadAsync(MarkUserInAppNotificationsReadDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = _currentUser.RequireGymId();
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User context is required.");
        return _repository.MarkReadAsync(gymId, userId, dto.NotificationIds, cancellationToken);
    }

    public async Task<SubscriptionExpiryNotificationGenerationResultDto> GenerateDailyNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var subscriptions = await _repository.GetActiveSubscriptionsForNotificationJobAsync(cancellationToken);
        var result = new SubscriptionExpiryNotificationGenerationResultDto
        {
            SubscriptionsProcessed = subscriptions.Count
        };

        foreach (var subscription in subscriptions)
        {
            if (!subscription.HasAccess && subscription.GraceEndsAt.HasValue
                && subscription.GraceEndsAt.Value.Date < today.Date)
            {
                continue;
            }

            var milestone = SubscriptionExpiryCalculator.ResolveDailyMilestone(subscription, today);
            if (milestone is null)
                continue;

            var users = await _repository.GetGymTenantUsersAsync(subscription.GymId, cancellationToken);
            foreach (var user in users)
            {
                var actionRoute = string.Equals(user.RoleName, RoleNames.GymAdmin, StringComparison.OrdinalIgnoreCase)
                    ? GymAdminRenewRoute
                    : null;

                var created = await _repository.CreateNotificationAsync(
                    subscription.GymId,
                    user.UserId,
                    milestone.NotificationKey,
                    milestone.NotificationType,
                    milestone.Title,
                    milestone.Message,
                    milestone.Severity,
                    actionRoute,
                    milestone.ShowLoginPopup,
                    cancellationToken);

                if (created)
                    result.NotificationsCreated++;
                else
                    result.NotificationsSkippedDuplicate++;
            }
        }

        return result;
    }
}
