namespace Gym.Infrastructure.Persistence;

[Obsolete("Use DatabaseMigrator.RunAsync instead.")]
public static class StoredProcedureDeployer
{
    public static Task DeployAsync(IServiceProvider serviceProvider) =>
        DatabaseMigrator.RunAsync(serviceProvider);
}
