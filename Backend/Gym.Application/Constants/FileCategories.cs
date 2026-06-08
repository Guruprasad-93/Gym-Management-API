namespace Gym.Application.Constants;

public static class FileCategories
{
    public const string GymLogo = "GymLogo";
    public const string GymBanner = "GymBanner";
    public const string MemberProfilePhoto = "MemberProfilePhoto";
    public const string TrainerProfilePhoto = "TrainerProfilePhoto";
    public const string MemberProgressPhoto = "MemberProgressPhoto";
    public const string DietAttachment = "DietAttachment";
    public const string WorkoutAttachment = "WorkoutAttachment";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        GymLogo, GymBanner, MemberProfilePhoto, TrainerProfilePhoto, MemberProgressPhoto, DietAttachment, WorkoutAttachment
    };

    public static bool IsImageCategory(string category) =>
        category is MemberProfilePhoto or TrainerProfilePhoto or MemberProgressPhoto or GymLogo or GymBanner;

    public static bool AllowsDocuments(string category) =>
        category is DietAttachment or WorkoutAttachment;
}
