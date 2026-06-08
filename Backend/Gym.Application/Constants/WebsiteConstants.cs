namespace Gym.Application.Constants;

public static class WebsiteSectionTypes
{
    public const string Hero = "Hero";
    public const string About = "About";
    public const string MembershipPlans = "MembershipPlans";
    public const string Trainers = "Trainers";
    public const string Testimonials = "Testimonials";
    public const string Gallery = "Gallery";
    public const string Contact = "Contact";
    public const string Cta = "CTA";

    public static readonly IReadOnlyList<string> All =
    [
        Hero, About, MembershipPlans, Trainers, Testimonials, Gallery, Contact, Cta
    ];
}

public static class WebsiteLeadCaptureStatuses
{
    public const string New = "New";
    public const string TrialScheduled = "TrialScheduled";
    public const string Converted = "Converted";
}
