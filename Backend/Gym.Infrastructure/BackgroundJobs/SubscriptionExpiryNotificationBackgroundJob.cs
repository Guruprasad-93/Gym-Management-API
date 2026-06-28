using Gym.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.BackgroundJobs;

public class SubscriptionExpiryNotificationBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryNotificationBackgroundJob> _logger;

    public SubscriptionExpiryNotificationBackgroundJob(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryNotificationBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnceAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextUtcMidnight();
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunOnceAsync(stoppingToken);
        }
    }

    internal static TimeSpan GetDelayUntilNextUtcMidnight()
    {
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        return nextMidnight - now;
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISubscriptionNotificationService>();
            var result = await service.GenerateDailyNotificationsAsync(stoppingToken);
            _logger.LogInformation(
                "Subscription expiry notification job completed. Processed={Processed}, Created={Created}, Skipped={Skipped}.",
                result.SubscriptionsProcessed,
                result.NotificationsCreated,
                result.NotificationsSkippedDuplicate);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Subscription expiry notification job failed.");
        }
    }
}
