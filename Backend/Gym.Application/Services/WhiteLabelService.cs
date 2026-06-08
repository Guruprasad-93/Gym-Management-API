using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Website;
using Gym.Application.DTOs.WhiteLabel;
using Gym.Application.Interfaces;
using Gym.Application.Validation;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class WhiteLabelService : IWhiteLabelService
{
    private readonly IWhiteLabelRepository _repository;
    private readonly IWebsiteRepository _websiteRepository;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public WhiteLabelService(
        IWhiteLabelRepository repository,
        IWebsiteRepository websiteRepository,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _websiteRepository = websiteRepository;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<WhiteLabelSettingsDto> UpsertSettingsAsync(UpsertWhiteLabelSettingsDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        WhiteLabelValidation.ValidateSettings(dto);
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpsertSettingsAsync(gymId, dto, cancellationToken);
        var settings = (await _repository.GetSettingsAsync(gymId, cancellationToken))!;
        await LogAuditAsync(gymId, AuditActionTypes.Update, settings, cancellationToken);
        return settings;
    }

    public async Task<WhiteLabelSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetSettingsAsync(gymId, cancellationToken)
            ?? new WhiteLabelSettingsDto { GymId = gymId, BrandName = string.Empty };
    }

    public async Task EnableAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.SetEnabledAsync(gymId, true, cancellationToken);
        await LogAuditAsync(gymId, AuditActionTypes.Activate, new { IsWhiteLabelEnabled = true }, cancellationToken);
    }

    public async Task DisableAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.SetEnabledAsync(gymId, false, cancellationToken);
        await LogAuditAsync(gymId, AuditActionTypes.Deactivate, new { IsWhiteLabelEnabled = false }, cancellationToken);
    }

    public async Task UpdateDomainAsync(UpdateWhiteLabelDomainDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateDomainAsync(gymId, dto, cancellationToken);
        await LogAuditAsync(gymId, AuditActionTypes.Update, dto, cancellationToken);
    }

    public Task<WhiteLabelLoginBrandingDto?> GetLoginBrandingAsync(WhiteLabelLoginBrandingQueryDto query, CancellationToken cancellationToken = default) =>
        _repository.GetLoginBrandingAsync(query, cancellationToken);

    public async Task<WhiteLabelPreviewDto> GetPreviewAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var settings = await _repository.GetSettingsAsync(gymId, cancellationToken)
            ?? new WhiteLabelSettingsDto { GymId = gymId, BrandName = "Preview Gym" };
        var mobile = await _repository.GetMobileSettingsAsync(gymId, cancellationToken)
            ?? new WhiteLabelMobileSettingsDto { GymId = gymId };
        var website = new WhiteLabelWebsitePreviewDto
        {
            BrandName = settings.BrandName,
            LogoUrl = settings.LogoUrl,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor
        };
        var site = await _websiteRepository.GetSettingsAsync(gymId, cancellationToken);
        if (site is not null)
        {
            website.WebsiteTitle = site.WebsiteTitle ?? settings.BrandName;
            if (!string.IsNullOrWhiteSpace(site.LogoUrl)) website.LogoUrl = site.LogoUrl;
            if (!string.IsNullOrWhiteSpace(site.PrimaryColor)) website.PrimaryColor = site.PrimaryColor;
            if (!string.IsNullOrWhiteSpace(site.SecondaryColor)) website.SecondaryColor = site.SecondaryColor;
        }

        return new WhiteLabelPreviewDto
        {
            Login = new WhiteLabelLoginBrandingDto
            {
                GymId = gymId,
                BrandName = settings.BrandName,
                AppDisplayName = settings.AppDisplayName ?? settings.BrandName,
                PrimaryColor = settings.PrimaryColor,
                SecondaryColor = settings.SecondaryColor,
                SupportEmail = settings.SupportEmail,
                SupportPhone = settings.SupportPhone,
                LogoUrl = settings.LogoUrl,
                LoginBackgroundUrl = settings.LoginBackgroundUrl
            },
            Website = website,
            Mobile = mobile
        };
    }

    public async Task ApplyWebsiteBrandingDefaultsAsync(Guid gymId, WhiteLabelWebsitePreviewDto target, CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSettingsAsync(gymId, cancellationToken);
        if (settings is null || !settings.IsWhiteLabelEnabled) return;
        target.BrandName ??= settings.BrandName;
        target.LogoUrl ??= settings.LogoUrl;
        target.PrimaryColor ??= settings.PrimaryColor;
        target.SecondaryColor ??= settings.SecondaryColor;
        target.WebsiteTitle ??= settings.AppDisplayName ?? settings.BrandName;
    }

    public async Task ApplyWebsiteSettingsDefaultsAsync(Guid gymId, GymWebsiteSettingsDto settings, CancellationToken cancellationToken = default)
    {
        var whiteLabel = await _repository.GetSettingsAsync(gymId, cancellationToken);
        if (whiteLabel is null || !whiteLabel.IsWhiteLabelEnabled) return;
        if (string.IsNullOrWhiteSpace(settings.LogoUrl))
        {
            settings.LogoUrl = whiteLabel.LogoUrl;
            settings.LogoFileId ??= whiteLabel.LogoFileId;
        }
        if (string.IsNullOrWhiteSpace(settings.PrimaryColor))
            settings.PrimaryColor = whiteLabel.PrimaryColor;
        if (string.IsNullOrWhiteSpace(settings.SecondaryColor))
            settings.SecondaryColor = whiteLabel.SecondaryColor;
        if (string.IsNullOrWhiteSpace(settings.WebsiteTitle))
            settings.WebsiteTitle = whiteLabel.AppDisplayName ?? whiteLabel.BrandName;
    }

    public async Task EnrichNotificationVariablesAsync(Guid gymId, IDictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetSettingsAsync(gymId, cancellationToken);
        if (settings is null || !settings.IsWhiteLabelEnabled) return;
        variables.TryAdd("brandName", settings.BrandName);
        if (!string.IsNullOrWhiteSpace(settings.AppDisplayName))
            variables.TryAdd("appDisplayName", settings.AppDisplayName);
        if (!string.IsNullOrWhiteSpace(settings.SupportEmail))
            variables.TryAdd("supportEmail", settings.SupportEmail);
        if (!string.IsNullOrWhiteSpace(settings.SupportPhone))
            variables.TryAdd("supportPhone", settings.SupportPhone);
        if (!string.IsNullOrWhiteSpace(settings.PrimaryColor))
            variables.TryAdd("primaryColor", settings.PrimaryColor);
    }

    public async Task<WhiteLabelEmailTemplateDto> CreateEmailTemplateAsync(UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var id = await _repository.CreateEmailTemplateAsync(gymId, dto, cancellationToken);
        return (await _repository.GetEmailTemplatesAsync(gymId, cancellationToken)).First(t => t.Id == id);
    }

    public async Task<WhiteLabelEmailTemplateDto> UpdateEmailTemplateAsync(UpdateWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateEmailTemplateAsync(gymId, dto, cancellationToken);
        return (await _repository.GetEmailTemplatesAsync(gymId, cancellationToken)).First(t => t.Id == dto.Id);
    }

    public async Task DeleteEmailTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.DeleteEmailTemplateAsync(gymId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<WhiteLabelEmailTemplateDto>> GetEmailTemplatesAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetEmailTemplatesAsync(gymId, cancellationToken);
    }

    public async Task<WhiteLabelMobileSettingsDto> UpsertMobileSettingsAsync(UpsertWhiteLabelMobileSettingsDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpsertMobileSettingsAsync(gymId, dto, cancellationToken);
        return (await _repository.GetMobileSettingsAsync(gymId, cancellationToken))!;
    }

    public async Task<WhiteLabelMobileSettingsDto?> GetMobileSettingsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetMobileSettingsAsync(gymId, cancellationToken);
    }

    public Task<WhiteLabelPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasRole(RoleNames.SuperAdmin))
            throw new UnauthorizedAccessException("Super Admin access required.");
        return _repository.GetPlatformDashboardAsync(cancellationToken);
    }

    private async Task LogAuditAsync(Guid gymId, string action, object value, CancellationToken cancellationToken) =>
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.WhiteLabelSettings,
            EntityId = gymId.ToString(),
            ActionType = action,
            NewValue = value
        }, cancellationToken);

    private void EnsureCanView()
    {
        if (!_currentUser.HasPermission(Permissions.ViewWhiteLabel))
            throw new UnauthorizedAccessException("Missing VIEW_WHITE_LABEL permission.");
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManageWhiteLabel))
            throw new UnauthorizedAccessException("Missing MANAGE_WHITE_LABEL permission.");
    }
}
