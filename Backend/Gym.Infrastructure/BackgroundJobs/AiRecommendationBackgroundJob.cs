using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.BackgroundJobs;

public class AiRecommendationBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AiSettings _settings;
    private readonly ILogger<AiRecommendationBackgroundJob> _logger;

    public AiRecommendationBackgroundJob(
        IServiceScopeFactory scopeFactory,
        IOptions<AiSettings> settings,
        ILogger<AiRecommendationBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(Math.Max(1, _settings.BackgroundJobIntervalHours));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var aiService = scope.ServiceProvider.GetRequiredService<IAiRecommendationService>();
                await aiService.RunDailyGenerationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI recommendation background job failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
