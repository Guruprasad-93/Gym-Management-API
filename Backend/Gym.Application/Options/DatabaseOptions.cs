namespace Gym.Application.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool RunMigrationsOnStartup { get; set; }
    public bool RunSeedOnStartup { get; set; } = true;
}
