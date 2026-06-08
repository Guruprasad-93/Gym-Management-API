using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights;
using Gym.API.Logging;

namespace Gym.API.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With(services.GetRequiredService<TenantLogEnricher>())
                .WriteTo.Console();

            var appInsightsConnection = context.Configuration["ApplicationInsights:ConnectionString"];
            if (context.Configuration.GetValue<bool>("ApplicationInsights:Enabled")
                && !string.IsNullOrWhiteSpace(appInsightsConnection))
            {
                configuration.WriteTo.ApplicationInsights(
                    appInsightsConnection,
                    TelemetryConverter.Traces);
            }
        });

        builder.Services.AddSingleton<TenantLogEnricher>();
        return builder;
    }
}
