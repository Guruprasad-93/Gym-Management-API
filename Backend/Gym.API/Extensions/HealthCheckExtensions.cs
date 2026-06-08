using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gym.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddGymHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddHealthChecks()
            .AddSqlServer(
                connectionString,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ready"])
            .AddCheck<Gym.API.HealthChecks.AzureBlobHealthCheck>(
                "azureblob",
                failureStatus: HealthStatus.Degraded,
                tags: ["storage", "ready"]);

        return services;
    }
}
