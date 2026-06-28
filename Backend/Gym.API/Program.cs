using Gym.API.Extensions;
using Gym.API.Middleware;
using Gym.Application.Options;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

if (args.Contains("migrate", StringComparer.OrdinalIgnoreCase))
{
    var migrateBuilder = WebApplication.CreateBuilder(args);
    migrateBuilder.AddEnvironmentConfiguration();
    migrateBuilder.Services.AddDbContextOnlyForMigration(migrateBuilder.Configuration);

    var migrateApp = migrateBuilder.Build();
    await DatabaseMigrator.RunAsync(migrateApp.Services);
    Log.Information("Database migration completed.");
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();
builder.AddEnvironmentConfiguration();
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();
app.ValidateProductionConfiguration();

var databaseOptions = app.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
    ?? new DatabaseOptions();

if (databaseOptions.RunMigrationsOnStartup)
    await DatabaseMigrator.RunAsync(app.Services);

if (databaseOptions.RunSeedOnStartup)
    await DatabaseSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("Frontend");
app.UseMiddleware<CsrfValidationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SubscriptionAccessMiddleware>();
app.UseMiddleware<FeatureAccessMiddleware>();
app.UseMiddleware<GymMenuAccessMiddleware>();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            }),
            totalDurationMs = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(payload);
    }
});

app.MapControllers();

app.Run();
