using System.Threading.RateLimiting;
using Gym.Application.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace Gym.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicyName = "AuthEndpoints";

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ?? new RateLimitSettings();
        var disableForTesting = configuration.GetValue<bool>("Testing:DisableRateLimiting");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthPolicyName, httpContext =>
                disableForTesting
                    ? RateLimitPartition.GetNoLimiter(GetClientPartitionKey(httpContext))
                    : RateLimitPartition.GetFixedWindowLimiter(
                        GetClientPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.AuthPermitLimit,
                            Window = TimeSpan.FromSeconds(settings.AuthWindowSeconds),
                            QueueLimit = 0
                        }));
        });

        return services;
    }

    private static string GetClientPartitionKey(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = httpContext.Request.Path.Value ?? "/";
        return $"{ip}:{path}";
    }
}
