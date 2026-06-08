using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Website;

namespace Gym.Application.Interfaces;

public interface IWebsiteRepository
{
    Task UpsertSettingsAsync(Guid gymId, UpsertGymWebsiteSettingsDto dto, CancellationToken cancellationToken = default);
    Task<GymWebsiteSettingsDto?> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task SetPublishedAsync(Guid gymId, bool isPublished, CancellationToken cancellationToken = default);
    Task<int> CreatePageAsync(Guid gymId, CreateGymWebsitePageDto dto, CancellationToken cancellationToken = default);
    Task UpdatePageAsync(Guid gymId, UpdateGymWebsitePageDto dto, CancellationToken cancellationToken = default);
    Task DeletePageAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsitePageDto>> GetPagesAsync(Guid gymId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<int> CreateSectionAsync(Guid gymId, CreateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default);
    Task UpdateSectionAsync(Guid gymId, UpdateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default);
    Task DeleteSectionAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteSectionDto>> GetSectionsAsync(Guid gymId, bool visibleOnly = false, CancellationToken cancellationToken = default);
    Task<int> CreateTestimonialAsync(Guid gymId, CreateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default);
    Task UpdateTestimonialAsync(Guid gymId, UpdateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default);
    Task DeleteTestimonialAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteTestimonialDto>> GetTestimonialsAsync(Guid gymId, bool approvedOnly = false, CancellationToken cancellationToken = default);
    Task<int> CreateGalleryItemAsync(Guid gymId, CreateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default);
    Task UpdateGalleryItemAsync(Guid gymId, UpdateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteGalleryItemAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteGalleryItemDto>> GetGalleryAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<int> CreateLeadCaptureAsync(Guid gymId, int? leadId, string name, string mobile, string? email, string source,
        string? interestedPlan, string? notes, string status, CancellationToken cancellationToken = default);
    Task<PagedResultDto<WebsiteLeadCaptureDto>> SearchLeadsAsync(Guid gymId, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebsiteLeadCaptureDto>> SearchAllLeadsAsync(Guid gymId, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task LinkLeadCaptureAsync(Guid gymId, int websiteLeadId, int leadId, CancellationToken cancellationToken = default);
    Task<PublicWebsiteDto?> GetPublicWebsiteAsync(string websiteSlug, CancellationToken cancellationToken = default);
    Task<Guid?> GetGymIdBySlugAsync(string websiteSlug, CancellationToken cancellationToken = default);
    Task<WebsiteAnalyticsOverviewDto> GetAnalyticsAsync(Guid gymId, int days, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WebsiteNotificationRecipientDto>> GetNotificationRecipientsAsync(Guid gymId, CancellationToken cancellationToken = default);
}

public interface IWebsiteService
{
    Task<GymWebsiteSettingsDto> UpsertSettingsAsync(UpsertGymWebsiteSettingsDto dto, CancellationToken cancellationToken = default);
    Task<GymWebsiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task PublishAsync(CancellationToken cancellationToken = default);
    Task UnpublishAsync(CancellationToken cancellationToken = default);
    Task<GymWebsitePageDto> CreatePageAsync(CreateGymWebsitePageDto dto, CancellationToken cancellationToken = default);
    Task<GymWebsitePageDto> UpdatePageAsync(UpdateGymWebsitePageDto dto, CancellationToken cancellationToken = default);
    Task DeletePageAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsitePageDto>> GetPagesAsync(CancellationToken cancellationToken = default);
    Task<GymWebsiteSectionDto> CreateSectionAsync(CreateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default);
    Task<GymWebsiteSectionDto> UpdateSectionAsync(UpdateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default);
    Task DeleteSectionAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteSectionDto>> GetSectionsAsync(CancellationToken cancellationToken = default);
    Task<GymWebsiteTestimonialDto> CreateTestimonialAsync(CreateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default);
    Task<GymWebsiteTestimonialDto> UpdateTestimonialAsync(UpdateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default);
    Task DeleteTestimonialAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteTestimonialDto>> GetTestimonialsAsync(CancellationToken cancellationToken = default);
    Task<GymWebsiteGalleryItemDto> CreateGalleryItemAsync(CreateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default);
    Task UpdateGalleryItemAsync(UpdateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteGalleryItemAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GymWebsiteGalleryItemDto>> GetGalleryAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<WebsiteLeadCaptureDto>> GetLeadsAsync(WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<WebsiteAnalyticsOverviewDto> GetAnalyticsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<byte[]> ExportLeadsAsync(string format, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<PublicWebsiteDto> GetPublicWebsiteAsync(string gymSlug, CancellationToken cancellationToken = default);
    Task<int> CapturePublicLeadAsync(PublicWebsiteLeadDto dto, CancellationToken cancellationToken = default);
    Task<int> BookPublicTrialAsync(PublicTrialBookingDto dto, CancellationToken cancellationToken = default);
    Task<string> GenerateSitemapAsync(string gymSlug, CancellationToken cancellationToken = default);
    Task<string> GenerateRobotsTxtAsync(string gymSlug, CancellationToken cancellationToken = default);
}

public interface IWebsiteReportExporter
{
    byte[] ExportLeadsPdf(IReadOnlyList<WebsiteLeadCaptureDto> leads, string title);
    byte[] ExportLeadsExcel(IReadOnlyList<WebsiteLeadCaptureDto> leads, string title);
}
