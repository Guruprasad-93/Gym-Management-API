using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.BackgroundJobs;

public class NotificationBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<NotificationBackgroundJob> _logger;

    public NotificationBackgroundJob(
        IServiceScopeFactory scopeFactory,
        IOptions<WhatsAppSettings> settings,
        ILogger<NotificationBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled && !string.Equals(_settings.Provider, "Mock", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("WhatsApp notifications disabled; background job will still queue expiry reminders in mock mode.");
        }

        var interval = TimeSpan.FromMinutes(Math.Max(5, _settings.BackgroundJobIntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.QueueMembershipExpiryRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification background job failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
