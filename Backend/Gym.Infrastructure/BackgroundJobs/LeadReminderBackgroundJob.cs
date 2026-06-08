using Gym.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.BackgroundJobs;

public class LeadReminderBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LeadReminderBackgroundJob> _logger;

    public LeadReminderBackgroundJob(
        IServiceScopeFactory scopeFactory,
        ILogger<LeadReminderBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
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
                var leadService = scope.ServiceProvider.GetRequiredService<ILeadService>();
                await leadService.ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lead reminder job failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
