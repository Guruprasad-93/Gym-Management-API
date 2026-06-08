using Azure.Storage.Blobs;
using Gym.Application.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Gym.API.HealthChecks;

public class AzureBlobHealthCheck : IHealthCheck
{
    private readonly FileStorageSettings _settings;

    public AzureBlobHealthCheck(IOptions<FileStorageSettings> settings) => _settings = settings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(_settings.Provider, "Azure", StringComparison.OrdinalIgnoreCase))
            return HealthCheckResult.Healthy("Blob check skipped (local file storage).");

        if (string.IsNullOrWhiteSpace(_settings.AzureConnectionString))
            return HealthCheckResult.Unhealthy("Azure Blob connection string is not configured.");

        try
        {
            var service = new BlobServiceClient(_settings.AzureConnectionString);
            var container = service.GetBlobContainerClient(_settings.AzureContainerName);
            var exists = await container.ExistsAsync(cancellationToken);
            return exists
                ? HealthCheckResult.Healthy("Azure Blob container is reachable.")
                : HealthCheckResult.Degraded("Azure Blob container does not exist yet.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Blob connectivity check failed.", ex);
        }
    }
}
