using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;
using Gym.Application.Services;
using Xunit;

namespace Gym.API.IntegrationTests;

public class SubscriptionExpiryCalculatorTests
{
    private static GymSubscriptionDto BuildSubscription(
        DateTime periodEnd,
        DateTime graceEnd,
        string status = "Active") =>
        new()
        {
            Id = 42,
            GymId = Guid.NewGuid(),
            Status = status,
            CurrentPeriodEnd = periodEnd,
            GraceEndsAt = graceEnd,
            HasAccess = true
        };

    [Theory]
    [InlineData("2026-06-23", "2026-06-30", "2026-07-03", SubscriptionNotificationTypes.Expiry7Days, "Your subscription expires in 7 days.")]
    [InlineData("2026-06-27", "2026-06-30", "2026-07-03", SubscriptionNotificationTypes.Expiry3Days, "Your subscription expires in 3 days.")]
    [InlineData("2026-06-28", "2026-06-30", "2026-07-03", SubscriptionNotificationTypes.Expiry2Days, "Your subscription expires in 2 days. Renew now to avoid interruption.")]
    [InlineData("2026-06-29", "2026-06-30", "2026-07-03", SubscriptionNotificationTypes.Expiry1Day, "Your subscription expires tomorrow.")]
    public void PreExpiryMilestones_AreResolved(
        string todayText,
        string periodEndText,
        string graceEndText,
        string expectedType,
        string expectedMessage)
    {
        var today = DateTime.Parse(todayText);
        var subscription = BuildSubscription(DateTime.Parse(periodEndText), DateTime.Parse(graceEndText));
        var milestone = SubscriptionExpiryCalculator.ResolveDailyMilestone(subscription, today);

        Assert.NotNull(milestone);
        Assert.Equal(expectedType, milestone!.NotificationType);
        Assert.Equal(expectedMessage, milestone.Message);
    }

    [Fact]
    public void ExpiryDayMilestone_IncludesGraceRemaining()
    {
        var subscription = BuildSubscription(new DateTime(2026, 6, 30), new DateTime(2026, 7, 3));
        var milestone = SubscriptionExpiryCalculator.ResolveDailyMilestone(subscription, new DateTime(2026, 6, 30));

        Assert.NotNull(milestone);
        Assert.Equal(SubscriptionNotificationTypes.ExpiredToday, milestone!.NotificationType);
        Assert.Contains("Your subscription has expired.", milestone.Message);
        Assert.Contains("Grace period remaining: 3 days.", milestone.Message);
    }

    [Theory]
    [InlineData("2026-06-02", "2026-06-01", "2026-06-04", SubscriptionNotificationTypes.Grace2DaysRemaining, "Grace period remaining: 2 days.")]
    [InlineData("2026-06-03", "2026-06-01", "2026-06-04", SubscriptionNotificationTypes.Grace1DayRemaining, "Grace period remaining: 1 day.")]
    [InlineData("2026-06-04", "2026-06-01", "2026-06-04", SubscriptionNotificationTypes.GraceLastDay, "Last day of grace period. Renew now.")]
    public void GraceMilestones_AreResolved(
        string todayText,
        string periodEndText,
        string graceEndText,
        string expectedType,
        string expectedMessage)
    {
        var subscription = BuildSubscription(DateTime.Parse(periodEndText), DateTime.Parse(graceEndText));
        subscription.HasAccess = true;
        var milestone = SubscriptionExpiryCalculator.ResolveDailyMilestone(subscription, DateTime.Parse(todayText));

        Assert.NotNull(milestone);
        Assert.Equal(expectedType, milestone!.NotificationType);
        Assert.Equal(expectedMessage, milestone.Message);
    }

    [Fact]
    public void BannerMessages_MatchRequirements()
    {
        Assert.Equal(
            "Your subscription expires in 2 days. Renew now to avoid interruption.",
            SubscriptionExpiryCalculator.ResolveBannerMessage(
                SubscriptionAccessModes.Active, 2, null, new DateTime(2026, 6, 30), new DateTime(2026, 6, 28)));

        Assert.Equal(
            "Last day of grace period. Renew now.",
            SubscriptionExpiryCalculator.ResolveBannerMessage(
                SubscriptionAccessModes.GracePeriod, 0, 0, new DateTime(2026, 7, 4), new DateTime(2026, 7, 4)));
    }
}
