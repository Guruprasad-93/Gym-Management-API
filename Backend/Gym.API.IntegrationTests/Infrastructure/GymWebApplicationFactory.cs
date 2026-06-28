using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gym.API.IntegrationTests.Infrastructure;

public class GymWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string IntegrationDatabase =
        "Server=.;Database=GymDb_LoginIdentifierTenantMenuE2E;Trusted_Connection=True;TrustServerCertificate=True";

    public bool DatabaseReady { get; private set; }
    public Guid DemoGymId { get; private set; } = DemoDataSeeder.DemoGymId;

    public GymWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Secret", "IntegrationTestSecretKey_AtLeast32Chars!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "GymManagementSystem");
        Environment.SetEnvironmentVariable("Jwt__Audience", "GymManagementSystem");
        Environment.SetEnvironmentVariable("Testing__DisableRateLimiting", "true");
        Environment.SetEnvironmentVariable("Testing__DisableCsrf", "true");
        Environment.SetEnvironmentVariable(
            "DATABASE_CONNECTION",
            IntegrationDatabase);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"),
                optional: false,
                reloadOnChange: false);

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = IntegrationDatabase,
                ["Database:RunMigrationsOnStartup"] = "false",
                ["Database:RunSeedOnStartup"] = "false",
                ["AuthCookies:UseCookieAuth"] = "true",
                ["Jwt:Secret"] = "IntegrationTestSecretKey_AtLeast32Chars!",
                ["Demo:Enabled"] = "true",
                ["Demo:Password"] = "Demo@123",
                ["Bootstrap:SuperAdminEmail"] = "superadmin@gym.com",
                ["Bootstrap:SuperAdminPassword"] = "SuperAdmin@123",
                ["Testing:DisableCsrf"] = "true",
                ["Testing:DisableRateLimiting"] = "true",
                ["Testing:SyncBootstrapPassword"] = "true",
                ["Razorpay:Enabled"] = "true",
                ["Razorpay:UseMockGateway"] = "true",
                ["Razorpay:KeyId"] = "rzp_test_mock",
                ["Razorpay:KeySecret"] = "mock_razorpay_dev_secret",
            });
        });
    }

    public async Task EnsureDatabaseAsync()
    {
        if (DatabaseReady)
            return;

        using var scope = Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connection = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Integration tests require ConnectionStrings:DefaultConnection.");

        await Gym.Infrastructure.Persistence.DatabaseMigrator.RunAsync(scope.ServiceProvider);
        await Gym.Infrastructure.Persistence.DatabaseSeeder.SeedAsync(scope.ServiceProvider);
        await Gym.Infrastructure.Persistence.DemoDataSeeder.SeedAsync(scope.ServiceProvider);
        DemoGymId = await DemoDataSeeder.ResolveDemoGymIdAsync(scope.ServiceProvider);

        var saasRepository = scope.ServiceProvider.GetRequiredService<ISaasSubscriptionRepository>();
        var saasSettings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Gym.Application.Options.SaasSubscriptionSettings>>().Value;
        var limit = await saasRepository.CheckTenantLimitAsync(DemoGymId, "Member");
        if (!limit.HasAccess)
            await saasRepository.CreateTrialSubscriptionAsync(DemoGymId, saasSettings.GracePeriodDays);
        await MvpDemoDataSeeder.EnsureDemoEnterpriseSubscriptionForGymAsync(
            saasRepository, DemoGymId, saasSettings.GracePeriodDays, CancellationToken.None);

        if (config.GetValue("Testing:SyncBootstrapPassword", false))
        {
            var bootstrapEmail = config["Bootstrap:SuperAdminEmail"]?.Trim().ToLowerInvariant();
            var bootstrapPassword = config["Bootstrap:SuperAdminPassword"];
            if (!string.IsNullOrWhiteSpace(bootstrapEmail) && !string.IsNullOrWhiteSpace(bootstrapPassword))
            {
                var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                var hash = passwordHasher.Hash(bootstrapPassword);
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE dbo.Users SET Password = {hash} WHERE Email = {bootstrapEmail} AND GymId IS NULL");
            }
        }

        DatabaseReady = true;
    }
}
