using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.BackgroundJobs;

public class SaasSubscriptionBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SaasSubscriptionSettings _settings;
    private readonly ILogger<SaasSubscriptionBackgroundJob> _logger;

    public SaasSubscriptionBackgroundJob(
        IServiceScopeFactory scopeFactory,
        IOptions<SaasSubscriptionSettings> settings,
        ILogger<SaasSubscriptionBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
                await repository.ExpireSubscriptionsAsync(_settings.GracePeriodDays, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaaS subscription expiry job failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
