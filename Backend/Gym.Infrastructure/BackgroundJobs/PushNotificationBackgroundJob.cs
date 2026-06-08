using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.BackgroundJobs;

public class PushNotificationBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FirebaseSettings _settings;
    private readonly ILogger<PushNotificationBackgroundJob> _logger;

    public PushNotificationBackgroundJob(
        IServiceScopeFactory scopeFactory,
        IOptions<FirebaseSettings> settings,
        ILogger<PushNotificationBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(60, _settings.BackgroundJobIntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mobilePushService = scope.ServiceProvider.GetRequiredService<IMobilePushService>();
                await mobilePushService.QueueScheduledRemindersAsync(stoppingToken);
                await mobilePushService.ProcessPendingPushNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push notification background job failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
