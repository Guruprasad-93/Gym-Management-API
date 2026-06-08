using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gym.API.IntegrationTests.Infrastructure;

public class GymWebApplicationFactory : WebApplicationFactory<Program>
{
    public bool DatabaseReady { get; private set; }

    public GymWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Secret", "IntegrationTestSecretKey_AtLeast32Chars!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "GymManagementSystem");
        Environment.SetEnvironmentVariable("Jwt__Audience", "GymManagementSystem");
        Environment.SetEnvironmentVariable(
            "DATABASE_CONNECTION",
            "Server=.;Database=GymDb_FreshSprintFix;Trusted_Connection=True;TrustServerCertificate=True");
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
                ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=GymDb_FreshSprintFix;Trusted_Connection=True;TrustServerCertificate=True",
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
        DatabaseReady = true;
    }
}
