using Gym.Application.DTOs.WhiteLabel;

namespace Gym.Application.Interfaces;

public interface IWhiteLabelRepository
{
    Task<int> UpsertSettingsAsync(Guid gymId, UpsertWhiteLabelSettingsDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelSettingsDto?> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task SetEnabledAsync(Guid gymId, bool enabled, CancellationToken cancellationToken = default);
    Task UpdateDomainAsync(Guid gymId, UpdateWhiteLabelDomainDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelSettingsDto?> GetBySubDomainAsync(string subDomain, CancellationToken cancellationToken = default);
    Task<WhiteLabelLoginBrandingDto?> GetLoginBrandingAsync(WhiteLabelLoginBrandingQueryDto query, CancellationToken cancellationToken = default);
    Task<int> CreateEmailTemplateAsync(Guid gymId, UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task UpdateEmailTemplateAsync(Guid gymId, UpdateWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task DeleteEmailTemplateAsync(Guid gymId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WhiteLabelEmailTemplateDto>> GetEmailTemplatesAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<int> UpsertMobileSettingsAsync(Guid gymId, UpsertWhiteLabelMobileSettingsDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelMobileSettingsDto?> GetMobileSettingsAsync(Guid gymId, CancellationToken cancellationToken = default);
    Task<WhiteLabelPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default);
}

public interface IWhiteLabelService
{
    Task<WhiteLabelSettingsDto> UpsertSettingsAsync(UpsertWhiteLabelSettingsDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task EnableAsync(CancellationToken cancellationToken = default);
    Task DisableAsync(CancellationToken cancellationToken = default);
    Task UpdateDomainAsync(UpdateWhiteLabelDomainDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelLoginBrandingDto?> GetLoginBrandingAsync(WhiteLabelLoginBrandingQueryDto query, CancellationToken cancellationToken = default);
    Task<WhiteLabelLoginBrandingDto> GetAppBrandingAsync(CancellationToken cancellationToken = default);
    Task<WhiteLabelPreviewDto> GetPreviewAsync(CancellationToken cancellationToken = default);
    Task ApplyWebsiteBrandingDefaultsAsync(Guid gymId, WhiteLabelWebsitePreviewDto target, CancellationToken cancellationToken = default);
    Task ApplyWebsiteSettingsDefaultsAsync(Guid gymId, Gym.Application.DTOs.Website.GymWebsiteSettingsDto settings, CancellationToken cancellationToken = default);
    Task EnrichNotificationVariablesAsync(Guid gymId, IDictionary<string, string> variables, CancellationToken cancellationToken = default);
    Task<WhiteLabelEmailTemplateDto> CreateEmailTemplateAsync(UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelEmailTemplateDto> UpdateEmailTemplateAsync(UpdateWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default);
    Task DeleteEmailTemplateAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WhiteLabelEmailTemplateDto>> GetEmailTemplatesAsync(CancellationToken cancellationToken = default);
    Task<WhiteLabelMobileSettingsDto> UpsertMobileSettingsAsync(UpsertWhiteLabelMobileSettingsDto dto, CancellationToken cancellationToken = default);
    Task<WhiteLabelMobileSettingsDto?> GetMobileSettingsAsync(CancellationToken cancellationToken = default);
    Task<WhiteLabelPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default);
}
