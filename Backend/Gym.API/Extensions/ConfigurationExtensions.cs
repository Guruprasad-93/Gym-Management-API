namespace Gym.API.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Maps common environment variables into configuration (supports Docker/K8s secret injection).
    /// </summary>
    public static WebApplicationBuilder AddEnvironmentConfiguration(this WebApplicationBuilder builder)
    {
        var overrides = new Dictionary<string, string?>();

        void Map(string envName, string configKey)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(value))
                overrides[configKey] = value;
        }

        Map("JWT_SECRET", "Jwt:Secret");
        Map("JWT_ISSUER", "Jwt:Issuer");
        Map("JWT_AUDIENCE", "Jwt:Audience");
        Map("DATABASE_CONNECTION", "ConnectionStrings:DefaultConnection");
        Map("BOOTSTRAP_SUPERADMIN_PASSWORD", "Bootstrap:SuperAdminPassword");
        Map("BOOTSTRAP_SUPERADMIN_EMAIL", "Bootstrap:SuperAdminEmail");
        Map("PASSWORD_RESET_FRONTEND_URL", "PasswordReset:FrontendBaseUrl");
        Map("FILE_STORAGE_URL_SIGNING_SECRET", "FileStorage:UrlSigningSecret");
        Map("AZURE_BLOB_CONNECTION", "FileStorage:AzureConnectionString");
        Map("APPLICATIONINSIGHTS_CONNECTION_STRING", "ApplicationInsights:ConnectionString");
        Map("DATABASE_RUN_MIGRATIONS_ON_STARTUP", "Database:RunMigrationsOnStartup");
        Map("DATABASE_RUN_SEED_ON_STARTUP", "Database:RunSeedOnStartup");

        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrWhiteSpace(corsOrigins))
        {
            var index = 0;
            foreach (var origin in corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                overrides[$"Cors:AllowedOrigins:{index++}"] = origin;
        }

        if (overrides.Count > 0)
            builder.Configuration.AddInMemoryCollection(overrides);

        return builder;
    }

    public static void ValidateProductionConfiguration(this WebApplication app)
    {
        if (!app.Environment.IsProduction())
            return;

        var config = app.Configuration;
        var jwtSecret = config["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            throw new InvalidOperationException("Production requires Jwt:Secret (or JWT_SECRET) of at least 32 characters.");

        if (config.GetValue<bool>("Jwt:ReturnResetTokenInDevelopment"))
            throw new InvalidOperationException("Jwt:ReturnResetTokenInDevelopment must be false in Production.");

        if (config.GetValue<bool>("Demo:Enabled"))
            throw new InvalidOperationException("Demo:Enabled must be false in Production.");

        var conn = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Production requires ConnectionStrings:DefaultConnection.");
    }

}
