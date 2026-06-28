using Gym.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.BackgroundJobs;

public class AttendanceAutoCheckoutBackgroundJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AttendanceAutoCheckoutBackgroundJob> _logger;

    public AttendanceAutoCheckoutBackgroundJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AttendanceAutoCheckoutBackgroundJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                var processed = await attendanceService.RunAutoCheckoutProcessingAsync(stoppingToken);
                if (processed > 0)
                    _logger.LogInformation("Auto check-out closed {Count} open attendance sessions.", processed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Attendance auto check-out background job failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
