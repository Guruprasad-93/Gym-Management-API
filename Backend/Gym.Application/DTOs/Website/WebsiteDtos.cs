using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Analytics;
using Gym.Application.DTOs.Memberships;

namespace Gym.Application.DTOs.Website;

public sealed class GymWebsiteSettingsDto
{
    public Guid GymId { get; set; }
    public string WebsiteSlug { get; set; } = string.Empty;
    public string? WebsiteTitle { get; set; }
    public string? WebsiteDescription { get; set; }
    public long? LogoFileId { get; set; }
    public long? BannerFileId { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Address { get; set; }
    public string? GoogleMapEmbedUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
}

public class UpsertGymWebsiteSettingsDto
{
    public string WebsiteSlug { get; set; } = string.Empty;
    public string? WebsiteTitle { get; set; }
    public string? WebsiteDescription { get; set; }
    public long? LogoFileId { get; set; }
    public long? BannerFileId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Address { get; set; }
    public string? GoogleMapEmbedUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
}

public sealed class GymWebsitePageDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PageContent { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateGymWebsitePageDto
{
    public string PageName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PageContent { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class UpdateGymWebsitePageDto : CreateGymWebsitePageDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class GymWebsiteSectionDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public long? ImageFileId { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; }
}

public class CreateGymWebsiteSectionDto
{
    public string SectionType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public long? ImageFileId { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class UpdateGymWebsiteSectionDto : CreateGymWebsiteSectionDto
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
}

public sealed class GymWebsiteTestimonialDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public long? ImageFileId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsApproved { get; set; }
}

public class CreateGymWebsiteTestimonialDto
{
    public string MemberName { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string? ReviewText { get; set; }
    public long? ImageFileId { get; set; }
    public bool IsApproved { get; set; }
}

public sealed class UpdateGymWebsiteTestimonialDto : CreateGymWebsiteTestimonialDto
{
    public int Id { get; set; }
}

public sealed class GymWebsiteGalleryItemDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public long FileId { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public string? PublicUrl { get; set; }
    public string? OriginalFileName { get; set; }
}

public class CreateGymWebsiteGalleryItemDto
{
    public long FileId { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class UpdateGymWebsiteGalleryItemDto
{
    public int Id { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class WebsiteLeadCaptureDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int? LeadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? InterestedPlan { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LeadStatus { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class WebsiteLeadSearchQueryDto : PagedRequestDto
{
    public new string? Search { get; set; }
    public string? Status { get; set; }
}

public sealed class PublicWebsiteLeadDto
{
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? InterestedPlan { get; set; }
    public string? Notes { get; set; }
    public string WebsiteSlug { get; set; } = string.Empty;
}

public sealed class PublicTrialBookingDto
{
    public string WebsiteSlug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime PreferredDate { get; set; }
    public TimeSpan PreferredTime { get; set; }
}

public sealed class PublicWebsiteTrainerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
}

public sealed class PublicWebsiteDto
{
    public GymWebsiteSettingsDto Settings { get; set; } = new();
    public string GymName { get; set; } = string.Empty;
    public IReadOnlyList<GymWebsiteSectionDto> Sections { get; set; } = [];
    public IReadOnlyList<GymWebsitePageDto> Pages { get; set; } = [];
    public IReadOnlyList<GymWebsiteTestimonialDto> Testimonials { get; set; } = [];
    public IReadOnlyList<GymWebsiteGalleryItemDto> Gallery { get; set; } = [];
    public IReadOnlyList<MembershipPlanResponseDto> MembershipPlans { get; set; } = [];
    public IReadOnlyList<PublicWebsiteTrainerDto> Trainers { get; set; } = [];
}

public sealed class WebsiteAnalyticsOverviewDto
{
    public int TotalWebsiteLeads { get; set; }
    public int TrialRequests { get; set; }
    public int ConvertedLeads { get; set; }
    public decimal LeadConversionRate { get; set; }
    public int LeadsInPeriod { get; set; }
    public IReadOnlyList<WebsiteDailyLeadDto> DailyLeads { get; set; } = [];
    public IReadOnlyList<NamedCountDto> TopSources { get; set; } = [];
}

public sealed class WebsiteDailyLeadDto
{
    public DateTime LeadDate { get; set; }
    public int LeadCount { get; set; }
}

public sealed class WebsiteNotificationRecipientDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string RecipientType { get; set; } = string.Empty;
}
