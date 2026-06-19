using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Persistence;

/// <summary>
/// Ensures every gym has an active SaaS subscription (trial if missing or expired).
/// </summary>
public static class GymSubscriptionSeeder
{
    public static async Task EnsureAllGymsHaveAccessAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var gymRepository = services.GetRequiredService<IGymRepository>();
        var saasRepository = services.GetRequiredService<ISaasSubscriptionRepository>();
        var settings = services.GetRequiredService<IOptions<SaasSubscriptionSettings>>().Value;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        var gyms = await gymRepository.GetAllAsync();
        foreach (var gym in gyms)
        {
            var check = await saasRepository.CheckTenantLimitAsync(gym.Id, "Member");
            if (check.HasAccess)
                continue;

            await saasRepository.CreateTrialSubscriptionAsync(gym.Id, settings.GracePeriodDays);
            logger.LogWarning(
                "Gym {GymName} ({GymId}) had no active subscription — started a new trial.",
                gym.Name,
                gym.Id);
        }
    }
}
