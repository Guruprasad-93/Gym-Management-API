namespace Gym.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string GymAdmin = "GymAdmin";
    public const string Trainer = "Trainer";
    public const string Member = "Member";

    public static readonly string[] UserRegistrationAllowed = [SuperAdmin, GymAdmin];
}
