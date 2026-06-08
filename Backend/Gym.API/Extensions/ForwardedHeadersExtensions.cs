using Microsoft.AspNetCore.HttpOverrides;

namespace Gym.API.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddAzureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
