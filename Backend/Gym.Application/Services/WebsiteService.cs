using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Leads;
using Gym.Application.DTOs.Notifications;
using Gym.Application.DTOs.Website;
using Gym.Application.Interfaces;

namespace Gym.Application.Services;

public class WebsiteService : IWebsiteService
{
    private readonly IWebsiteRepository _repository;
    private readonly ILeadRepository _leadRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebsiteReportExporter _exporter;
    private readonly IWhiteLabelService _whiteLabelService;

    public WebsiteService(
        IWebsiteRepository repository,
        ILeadRepository leadRepository,
        INotificationService notificationService,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IWebsiteReportExporter exporter,
        IWhiteLabelService whiteLabelService)
    {
        _repository = repository;
        _leadRepository = leadRepository;
        _notificationService = notificationService;
        _auditService = auditService;
        _currentUser = currentUser;
        _exporter = exporter;
        _whiteLabelService = whiteLabelService;
    }

    public async Task<GymWebsiteSettingsDto> UpsertSettingsAsync(UpsertGymWebsiteSettingsDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpsertSettingsAsync(gymId, dto, cancellationToken);
        return (await _repository.GetSettingsAsync(gymId, cancellationToken))!;
    }

    public async Task<GymWebsiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetSettingsAsync(gymId, cancellationToken)
            ?? new GymWebsiteSettingsDto { GymId = gymId };
    }

    public async Task PublishAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.SetPublishedAsync(gymId, true, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.GymWebsite,
            EntityId = gymId.ToString(),
            ActionType = AuditActionTypes.Activate,
            NewValue = new { IsPublished = true }
        }, cancellationToken);
    }

    public async Task UnpublishAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.SetPublishedAsync(gymId, false, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.GymWebsite,
            EntityId = gymId.ToString(),
            ActionType = AuditActionTypes.Deactivate,
            NewValue = new { IsPublished = false }
        }, cancellationToken);
    }

    public async Task<GymWebsitePageDto> CreatePageAsync(CreateGymWebsitePageDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var id = await _repository.CreatePageAsync(gymId, dto, cancellationToken);
        var page = (await _repository.GetPagesAsync(gymId, cancellationToken: cancellationToken)).First(p => p.Id == id);
        await LogPageAuditAsync(gymId, id.ToString(), AuditActionTypes.Create, page, cancellationToken);
        return page;
    }

    public async Task<GymWebsitePageDto> UpdatePageAsync(UpdateGymWebsitePageDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdatePageAsync(gymId, dto, cancellationToken);
        var page = (await _repository.GetPagesAsync(gymId, cancellationToken: cancellationToken)).First(p => p.Id == dto.Id);
        await LogPageAuditAsync(gymId, dto.Id.ToString(), AuditActionTypes.Update, page, cancellationToken);
        return page;
    }

    public async Task DeletePageAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.DeletePageAsync(gymId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<GymWebsitePageDto>> GetPagesAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetPagesAsync(gymId, cancellationToken: cancellationToken);
    }

    public async Task<GymWebsiteSectionDto> CreateSectionAsync(CreateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        if (!WebsiteSectionTypes.All.Contains(dto.SectionType))
            throw new ArgumentException($"Invalid section type: {dto.SectionType}");
        var id = await _repository.CreateSectionAsync(gymId, dto, cancellationToken);
        return (await _repository.GetSectionsAsync(gymId, cancellationToken: cancellationToken)).First(s => s.Id == id);
    }

    public async Task<GymWebsiteSectionDto> UpdateSectionAsync(UpdateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateSectionAsync(gymId, dto, cancellationToken);
        return (await _repository.GetSectionsAsync(gymId, cancellationToken: cancellationToken)).First(s => s.Id == dto.Id);
    }

    public async Task DeleteSectionAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.DeleteSectionAsync(gymId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<GymWebsiteSectionDto>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetSectionsAsync(gymId, cancellationToken: cancellationToken);
    }

    public async Task<GymWebsiteTestimonialDto> CreateTestimonialAsync(CreateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var id = await _repository.CreateTestimonialAsync(gymId, dto, cancellationToken);
        return (await _repository.GetTestimonialsAsync(gymId, cancellationToken: cancellationToken)).First(t => t.Id == id);
    }

    public async Task<GymWebsiteTestimonialDto> UpdateTestimonialAsync(UpdateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateTestimonialAsync(gymId, dto, cancellationToken);
        return (await _repository.GetTestimonialsAsync(gymId, cancellationToken: cancellationToken)).First(t => t.Id == dto.Id);
    }

    public async Task DeleteTestimonialAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.DeleteTestimonialAsync(gymId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<GymWebsiteTestimonialDto>> GetTestimonialsAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetTestimonialsAsync(gymId, cancellationToken: cancellationToken);
    }

    public async Task<GymWebsiteGalleryItemDto> CreateGalleryItemAsync(CreateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var id = await _repository.CreateGalleryItemAsync(gymId, dto, cancellationToken);
        return (await _repository.GetGalleryAsync(gymId, cancellationToken)).First(g => g.Id == id);
    }

    public async Task UpdateGalleryItemAsync(UpdateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.UpdateGalleryItemAsync(gymId, dto, cancellationToken);
    }

    public async Task DeleteGalleryItemAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        await _repository.DeleteGalleryItemAsync(gymId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<GymWebsiteGalleryItemDto>> GetGalleryAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetGalleryAsync(gymId, cancellationToken);
    }

    public async Task<PagedResultDto<WebsiteLeadCaptureDto>> GetLeadsAsync(WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanView();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.SearchLeadsAsync(gymId, query, cancellationToken);
    }

    public async Task<WebsiteAnalyticsOverviewDto> GetAnalyticsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        return await _repository.GetAnalyticsAsync(gymId, days, cancellationToken);
    }

    public async Task<byte[]> ExportLeadsAsync(string format, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var gymId = GymScopeResolver.ResolveRequired(_currentUser, null);
        var leads = await _repository.SearchAllLeadsAsync(gymId, query, cancellationToken);
        var title = "Website Leads Report";
        var bytes = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? _exporter.ExportLeadsExcel(leads, title)
            : _exporter.ExportLeadsPdf(leads, title);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.WebsiteLeadCapture,
            EntityId = "export",
            ActionType = AuditActionTypes.Export,
            NewValue = new { format }
        }, cancellationToken);
        return bytes;
    }

    public async Task<PublicWebsiteDto> GetPublicWebsiteAsync(string gymSlug, CancellationToken cancellationToken = default)
    {
        var site = await _repository.GetPublicWebsiteAsync(gymSlug, cancellationToken)
            ?? throw new KeyNotFoundException("Website not found or not published.");
        await _whiteLabelService.ApplyWebsiteSettingsDefaultsAsync(site.Settings.GymId, site.Settings, cancellationToken);
        return site;
    }

    public async Task<int> CapturePublicLeadAsync(PublicWebsiteLeadDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolvePublishedGymIdAsync(dto.WebsiteSlug, cancellationToken);
        var lead = await _leadRepository.CreateAsync(gymId, null, new CreateLeadDto
        {
            FullName = dto.Name.Trim(),
            MobileNumber = dto.MobileNumber.Trim(),
            Email = dto.Email,
            LeadSource = LeadSources.Website,
            Notes = dto.Notes
        }, cancellationToken);

        await _leadRepository.CreateActivityAsync(lead.Id, gymId, LeadActivityTypes.Created,
            "Lead captured from public website.", null, cancellationToken);

        var captureId = await _repository.CreateLeadCaptureAsync(gymId, lead.Id, dto.Name, dto.MobileNumber, dto.Email,
            LeadSources.Website, dto.InterestedPlan, dto.Notes, WebsiteLeadCaptureStatuses.New, cancellationToken);

        await NotifyAdminsAsync(gymId, NotificationTypes.WebsiteLeadCreated, new Dictionary<string, string>
        {
            ["leadName"] = dto.Name,
            ["mobileNumber"] = dto.MobileNumber,
            ["interestedPlan"] = dto.InterestedPlan ?? "N/A"
        }, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.WebsiteLeadCapture,
            EntityId = captureId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = dto
        }, cancellationToken);

        return captureId;
    }

    public async Task<int> BookPublicTrialAsync(PublicTrialBookingDto dto, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolvePublishedGymIdAsync(dto.WebsiteSlug, cancellationToken);
        var trialDateTime = dto.PreferredDate.Date.Add(dto.PreferredTime);
        var notes = $"Preferred trial: {dto.PreferredDate:yyyy-MM-dd} at {dto.PreferredTime:hh\\:mm}";

        var lead = await _leadRepository.CreateAsync(gymId, null, new CreateLeadDto
        {
            FullName = dto.Name.Trim(),
            MobileNumber = dto.MobileNumber.Trim(),
            Email = dto.Email,
            LeadSource = LeadSources.Website,
            Status = LeadStatuses.TrialScheduled,
            Notes = notes
        }, cancellationToken);

        await _leadRepository.ScheduleTrialAsync(lead.Id, gymId, null, trialDateTime, null, cancellationToken);
        await _leadRepository.CreateActivityAsync(lead.Id, gymId, LeadActivityTypes.TrialScheduled, notes, null, cancellationToken);

        var captureId = await _repository.CreateLeadCaptureAsync(gymId, lead.Id, dto.Name, dto.MobileNumber, dto.Email,
            LeadSources.Website, null, notes, WebsiteLeadCaptureStatuses.TrialScheduled, cancellationToken);

        await NotifyAdminsAsync(gymId, NotificationTypes.TrialBooked, new Dictionary<string, string>
        {
            ["leadName"] = dto.Name,
            ["trialDate"] = dto.PreferredDate.ToString("yyyy-MM-dd"),
            ["trialTime"] = dto.PreferredTime.ToString(@"hh\:mm")
        }, cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.WebsiteLeadCapture,
            EntityId = captureId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = dto
        }, cancellationToken);

        return captureId;
    }

    public async Task<string> GenerateSitemapAsync(string gymSlug, CancellationToken cancellationToken = default)
    {
        var site = await GetPublicWebsiteAsync(gymSlug, cancellationToken);
        var baseUrl = $"/website/{site.Settings.WebsiteSlug}";
        var urls = new List<string> { baseUrl, $"{baseUrl}/about", $"{baseUrl}/plans", $"{baseUrl}/trainers", $"{baseUrl}/gallery", $"{baseUrl}/contact" };
        urls.AddRange(site.Pages.Select(p => $"{baseUrl}/{p.Slug}"));
        var body = string.Join(Environment.NewLine, urls.Distinct().Select(u =>
            $"  <url><loc>{u}</loc><changefreq>weekly</changefreq></url>"));
        return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">{Environment.NewLine}{body}{Environment.NewLine}</urlset>";
    }

    public Task<string> GenerateRobotsTxtAsync(string gymSlug, CancellationToken cancellationToken = default) =>
        Task.FromResult($"User-agent: *\nAllow: /website/{gymSlug}\nSitemap: /api/public/website/{gymSlug}/sitemap");

    private async Task<Guid> ResolvePublishedGymIdAsync(string slug, CancellationToken cancellationToken)
    {
        var gymId = await _repository.GetGymIdBySlugAsync(slug, cancellationToken);
        if (gymId is null)
            throw new KeyNotFoundException("Website not found or not published.");
        return gymId.Value;
    }

    private async Task NotifyAdminsAsync(Guid gymId, string notificationType, Dictionary<string, string> variables, CancellationToken cancellationToken)
    {
        var recipients = await _repository.GetNotificationRecipientsAsync(gymId, cancellationToken);
        foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r.PhoneNumber)).DistinctBy(r => r.PhoneNumber))
        {
            await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
            {
                NotificationType = notificationType,
                PhoneNumber = recipient.PhoneNumber!,
                RecipientUserId = recipient.UserId,
                Variables = variables,
                RelatedEntityType = AuditEntityNames.WebsiteLeadCapture
            }, cancellationToken);
        }
    }

    private async Task LogPageAuditAsync(Guid gymId, string entityId, string action, object value, CancellationToken cancellationToken) =>
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.GymWebsitePage,
            EntityId = entityId,
            ActionType = action,
            NewValue = value
        }, cancellationToken);

    private void EnsureCanView()
    {
        if (!_currentUser.HasPermission(Permissions.ViewWebsiteBuilder))
            throw new UnauthorizedAccessException("Missing VIEW_WEBSITE_BUILDER permission.");
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManageWebsiteBuilder))
            throw new UnauthorizedAccessException("Missing MANAGE_WEBSITE_BUILDER permission.");
    }

    private void EnsureCanViewAnalytics()
    {
        if (!_currentUser.HasPermission(Permissions.ViewWebsiteAnalytics))
            throw new UnauthorizedAccessException("Missing VIEW_WEBSITE_ANALYTICS permission.");
    }
}
