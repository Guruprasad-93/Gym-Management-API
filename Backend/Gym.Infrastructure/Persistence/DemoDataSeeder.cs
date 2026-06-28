using Gym.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Persistence;

/// <summary>
/// Seeds a complete demo tenant for MVP presentations.
/// </summary>
public static class DemoDataSeeder
{
    public const string DemoGymName = "FitZone Demo Gym";
    public const string DemoGymAdminEmail = "priya.sharma@fitzonegym.in";
    public const string DemoGymAdminLoginIdentifier = "fitzone_admin";
    public const string DemoTrainer1LoginIdentifier = "fitzone_trainer1";
    public const string DemoTrainer2LoginIdentifier = "fitzone_trainer2";
    public const string DemoMember1LoginIdentifier = "fitzone_member001";
    public const string DemoMember2LoginIdentifier = "fitzone_member002";
    public static readonly Guid DemoGymId = Guid.Parse("b2edbb38-ee01-4d17-94b6-1b3303807b91");
    public const string DefaultDemoPassword = "Demo@123";

    public static async Task<Guid> ResolveDemoGymIdAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var gymRepository = scope.ServiceProvider.GetRequiredService<IGymRepository>();
        var gyms = await gymRepository.GetAllAsync(cancellationToken);
        var existingDemo = gyms.FirstOrDefault(g =>
            string.Equals(g.Name, DemoGymName, StringComparison.OrdinalIgnoreCase));
        return existingDemo?.Id ?? DemoGymId;
    }

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Demo:Enabled", false))
            return;

        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var gymRepository = services.GetRequiredService<IGymRepository>();
        var demoPassword = configuration["Demo:Password"] ?? DefaultDemoPassword;
        var resetOnStartup = configuration.GetValue("Demo:ResetOnStartup", false);
        var forceReseed = configuration.GetValue("Demo:ForceReseed", false);

        if (resetOnStartup || forceReseed)
        {
            await DemoDatabaseReset.ResetBusinessDataAsync(services, logger);
        }

        var existingDemo = (await gymRepository.GetAllAsync())
            .FirstOrDefault(g => string.Equals(g.Name, DemoGymName, StringComparison.OrdinalIgnoreCase));

        if (existingDemo is not null && !resetOnStartup && !forceReseed)
        {
            await EnsureDemoGymHasActiveSubscriptionAsync(services, existingDemo.Id, logger);
            await EnsureDemoGymMenusAsync(services, existingDemo.Id, logger);
            await EnsureDemoGymAdminAsync(services, existingDemo.Id, demoPassword, logger);
            await EnsureDemoLoginIdentifiersMigratedAsync(services, existingDemo.Id, logger);
            logger.LogInformation("Demo gym '{GymName}' already exists — skipping full MVP seed.", DemoGymName);
            return;
        }

        logger.LogInformation("Seeding MVP demo data for '{GymName}'...", DemoGymName);
        await MvpDemoDataSeeder.SeedAsync(services, DemoGymId, demoPassword, logger);

        var superAdminEmail = configuration["Bootstrap:SuperAdminEmail"] ?? "superadmin@gym.com";
        logger.LogInformation(
            "MVP demo ready. SuperAdmin: {SuperAdminEmail} / superadmin. GymAdmin: {GymAdminLogin} / {DemoPassword}. Trainers: fitzone_trainer1–5. Members: fitzone_member001–100.",
            superAdminEmail,
            DemoGymAdminLoginIdentifier,
            demoPassword);
    }

    private static async Task EnsureDemoGymMenusAsync(
        IServiceProvider services,
        Guid gymId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var gymMenuService = services.GetRequiredService<IGymMenuService>();
        await gymMenuService.SeedMenusForGymAsync(gymId, null, cancellationToken);
        logger.LogDebug("Ensured tenant menus exist for demo gym {GymId}.", gymId);
    }

    private static async Task EnsureDemoGymAdminAsync(
        IServiceProvider services,
        Guid gymId,
        string demoPassword,
        ILogger logger)
    {
        var gymAdminRepository = services.GetRequiredService<IGymAdminRepository>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        if (await userRepository.ExistsByLoginIdentifierAsync(DemoGymAdminLoginIdentifier))
            return;

        await gymAdminRepository.CreateAsync(
            Guid.NewGuid(),
            gymId,
            "Priya Sharma",
            DemoGymAdminLoginIdentifier,
            DemoGymAdminEmail,
            services.GetRequiredService<IPasswordHasher>().Hash(demoPassword),
            mustChangePassword: false);
        logger.LogInformation("Created missing demo gym admin for {GymId}.", gymId);
    }

    private static async Task EnsureDemoLoginIdentifiersMigratedAsync(
        IServiceProvider services,
        Guid gymId,
        ILogger logger)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var renames = new (string OldId, string NewId)[]
        {
            ("admin", DemoGymAdminLoginIdentifier),
            ("admin@fitzone-demo.com", DemoGymAdminLoginIdentifier),
            ("trainer1", DemoTrainer1LoginIdentifier),
            ("EMP001", DemoTrainer1LoginIdentifier),
            ("trainer2", DemoTrainer2LoginIdentifier),
            ("MEM000123", "fitzone_member001"),
            ("member1", "fitzone_member001"),
            ("fitzone_member1", "fitzone_member001"),
            ("9876543210", "fitzone_member002"),
            ("member2", "fitzone_member002"),
            ("fitzone_member2", "fitzone_member002"),
        };

        foreach (var (oldId, newId) in renames)
        {
            await db.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE LoginIdentifier = {0})
   AND EXISTS (SELECT 1 FROM dbo.Users WHERE GymId = {1} AND LoginIdentifier = {2})
BEGIN
    UPDATE dbo.Users SET LoginIdentifier = {0} WHERE GymId = {1} AND LoginIdentifier = {2};
END",
                newId, gymId, oldId);
        }

        logger.LogDebug("Ensured demo gym login identifiers are globally unique for {GymId}.", gymId);
    }

    private static async Task EnsureDemoGymHasActiveSubscriptionAsync(
        IServiceProvider services,
        Guid gymId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var saasRepository = services.GetRequiredService<ISaasSubscriptionRepository>();
        var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Gym.Application.Options.SaasSubscriptionSettings>>().Value;
        var limit = await saasRepository.CheckTenantLimitAsync(gymId, "Member", cancellationToken);
        if (!limit.HasAccess)
        {
            await saasRepository.CreateTrialSubscriptionAsync(gymId, settings.GracePeriodDays, cancellationToken);
            logger.LogWarning(
                "Demo gym subscription was missing or expired for {GymId} — started a new trial.",
                gymId);
        }

        await MvpDemoDataSeeder.EnsureDemoEnterpriseSubscriptionForGymAsync(
            saasRepository, gymId, settings.GracePeriodDays, cancellationToken);
    }
}
