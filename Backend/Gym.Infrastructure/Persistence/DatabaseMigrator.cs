using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    public static async Task RunAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Running EF Core migrations.");
        await context.Database.MigrateAsync(cancellationToken);

        var appliedScripts = await LoadAppliedScriptsAsync(context, cancellationToken);
        var assembly = typeof(DatabaseMigrator).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n)
            .ToList();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            foreach (var resourceName in resourceNames)
            {
                var scriptName = ExtractScriptName(resourceName);
                if (appliedScripts.Contains(scriptName))
                {
                    logger.LogDebug("Skipping already applied script {Script}.", scriptName);
                    continue;
                }

                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                    continue;

                using var reader = new StreamReader(stream);
                var sql = await reader.ReadToEndAsync(cancellationToken);

                foreach (var batch in SplitBatches(sql))
                {
                    if (string.IsNullOrWhiteSpace(batch))
                        continue;

                    try
                    {
                        await using var command = connection.CreateCommand();
                        command.CommandText = batch;
                        command.CommandTimeout = 120;
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"SQL migration script '{scriptName}' failed. See inner exception for batch details.",
                            ex);
                    }
                }

                await RecordScriptAppliedAsync(connection, scriptName, cancellationToken);
                appliedScripts.Add(scriptName);
                logger.LogInformation("Applied SQL script {Script}.", scriptName);
            }

            await ValidateCriticalSchemaAsync(connection, cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static string ExtractScriptName(string resourceName)
    {
        const string marker = ".Persistence.Scripts.";
        var index = resourceName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index >= 0 ? resourceName[(index + marker.Length)..] : resourceName;
    }

    private static async Task<HashSet<string>> LoadAppliedScriptsAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var check = connection.CreateCommand();
            check.CommandText = """
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersions'
                """;
            var exists = await check.ExecuteScalarAsync(cancellationToken);
            if (exists is null)
                return applied;

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT ScriptName FROM dbo.SchemaVersions";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                applied.Add(reader.GetString(0));
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }

        return applied;
    }

    private static async Task RecordScriptAppliedAsync(
        System.Data.Common.DbConnection connection,
        string scriptName,
        CancellationToken cancellationToken)
    {
        await EnsureSchemaVersionsTableAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE ScriptName = @ScriptName)
                INSERT INTO dbo.SchemaVersions (ScriptName) VALUES (@ScriptName);
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@ScriptName";
        parameter.Value = scriptName;
        command.Parameters.Add(parameter);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureSchemaVersionsTableAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.SchemaVersions
                (
                    VersionId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
                    ScriptName NVARCHAR(200) NOT NULL,
                    AppliedAt DATETIME2 NOT NULL CONSTRAINT DF_SchemaVersions_AppliedAt DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT UQ_SchemaVersions_ScriptName UNIQUE (ScriptName)
                );
            END
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IEnumerable<string> SplitBatches(string sql)
    {
        const string delimiter = "GO";
        var lines = sql.Replace("\r\n", "\n").Split('\n');
        var batch = new System.Text.StringBuilder();

        foreach (var line in lines)
        {
            if (line.Trim().Equals(delimiter, StringComparison.OrdinalIgnoreCase))
            {
                yield return batch.ToString();
                batch.Clear();
                continue;
            }

            batch.AppendLine(line);
        }

        if (batch.Length > 0)
            yield return batch.ToString();
    }

    private static async Task ValidateCriticalSchemaAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF COL_LENGTH(N'dbo.Users', N'LoginIdentifier') IS NULL
                THROW 51061, 'Critical schema validation failed: Users.LoginIdentifier column is missing.', 1;

            IF EXISTS (
                SELECT 1
                FROM sys.columns
                WHERE object_id = OBJECT_ID(N'dbo.Users')
                  AND name = N'Email'
                  AND is_nullable = 0
            )
                THROW 51062, 'Critical schema validation failed: Users.Email is still NOT NULL.', 1;

            IF NOT EXISTS (
                SELECT 1 FROM sys.procedures WHERE name = N'sp_LoginUser'
            )
                THROW 51063, 'Critical schema validation failed: sp_LoginUser is missing.', 1;

            IF OBJECT_ID(N'dbo.Members', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.Members', N'IsDeleted') IS NULL
                THROW 51064, 'Critical schema validation failed: Members.IsDeleted column is missing.', 1;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
