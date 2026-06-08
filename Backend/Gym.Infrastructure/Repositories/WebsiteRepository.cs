using System.Data;
using Dapper;
using Gym.Application.DTOs.Analytics;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Website;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class WebsiteRepository : IWebsiteRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public WebsiteRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public Task UpsertSettingsAsync(Guid gymId, UpsertGymWebsiteSettingsDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpsertGymWebsiteSettings, new
        {
            GymId = gymId,
            dto.WebsiteSlug,
            dto.WebsiteTitle,
            dto.WebsiteDescription,
            dto.LogoFileId,
            dto.BannerFileId,
            dto.PrimaryColor,
            dto.SecondaryColor,
            dto.ContactPhone,
            dto.ContactEmail,
            dto.WhatsAppNumber,
            dto.Address,
            dto.GoogleMapEmbedUrl,
            dto.FacebookUrl,
            dto.InstagramUrl,
            dto.YoutubeUrl,
            dto.MetaTitle,
            dto.MetaDescription,
            dto.MetaKeywords
        }, cancellationToken);

    public async Task<GymWebsiteSettingsDto?> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WebsiteSettingsRow>(
            StoredProcedureNames.GetGymWebsiteSettings, new { GymId = gymId }, cancellationToken);
        return row is null ? null : MapSettings(row);
    }

    public Task SetPublishedAsync(Guid gymId, bool isPublished, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetGymWebsitePublished, new { GymId = gymId, IsPublished = isPublished }, cancellationToken);

    public async Task<int> CreatePageAsync(Guid gymId, CreateGymWebsitePageDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@PageName", dto.PageName);
        parameters.Add("@Slug", dto.Slug);
        parameters.Add("@PageContent", dto.PageContent);
        parameters.Add("@DisplayOrder", dto.DisplayOrder);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateGymWebsitePage, parameters, "@Id", cancellationToken);
    }

    public Task UpdatePageAsync(Guid gymId, UpdateGymWebsitePageDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateGymWebsitePage, new
        {
            GymId = gymId,
            dto.Id,
            dto.PageName,
            dto.Slug,
            dto.PageContent,
            dto.DisplayOrder,
            dto.IsActive
        }, cancellationToken);

    public Task DeletePageAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteGymWebsitePage, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<GymWebsitePageDto>> GetPagesAsync(Guid gymId, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymWebsitePageRow>(StoredProcedureNames.GetGymWebsitePages,
            new { GymId = gymId, ActiveOnly = activeOnly }, cancellationToken);
        return rows.Select(MapPage).ToList();
    }

    public async Task<int> CreateSectionAsync(Guid gymId, CreateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@SectionType", dto.SectionType);
        parameters.Add("@Title", dto.Title);
        parameters.Add("@Subtitle", dto.Subtitle);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@ImageFileId", dto.ImageFileId);
        parameters.Add("@DisplayOrder", dto.DisplayOrder);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateGymWebsiteSection, parameters, "@Id", cancellationToken);
    }

    public Task UpdateSectionAsync(Guid gymId, UpdateGymWebsiteSectionDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateGymWebsiteSection, new
        {
            GymId = gymId,
            dto.Id,
            dto.SectionType,
            dto.Title,
            dto.Subtitle,
            dto.Description,
            dto.ImageFileId,
            dto.DisplayOrder,
            dto.IsVisible
        }, cancellationToken);

    public Task DeleteSectionAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteGymWebsiteSection, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<GymWebsiteSectionDto>> GetSectionsAsync(Guid gymId, bool visibleOnly = false, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymWebsiteSectionRow>(StoredProcedureNames.GetGymWebsiteSections,
            new { GymId = gymId, VisibleOnly = visibleOnly }, cancellationToken);
        return rows.Select(MapSection).ToList();
    }

    public async Task<int> CreateTestimonialAsync(Guid gymId, CreateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberName", dto.MemberName);
        parameters.Add("@Rating", dto.Rating);
        parameters.Add("@ReviewText", dto.ReviewText);
        parameters.Add("@ImageFileId", dto.ImageFileId);
        parameters.Add("@IsApproved", dto.IsApproved);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateGymWebsiteTestimonial, parameters, "@Id", cancellationToken);
    }

    public Task UpdateTestimonialAsync(Guid gymId, UpdateGymWebsiteTestimonialDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateGymWebsiteTestimonial, new
        {
            GymId = gymId,
            dto.Id,
            dto.MemberName,
            dto.Rating,
            dto.ReviewText,
            dto.ImageFileId,
            dto.IsApproved
        }, cancellationToken);

    public Task DeleteTestimonialAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteGymWebsiteTestimonial, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<GymWebsiteTestimonialDto>> GetTestimonialsAsync(Guid gymId, bool approvedOnly = false, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymWebsiteTestimonialRow>(StoredProcedureNames.GetGymWebsiteTestimonials,
            new { GymId = gymId, ApprovedOnly = approvedOnly }, cancellationToken);
        return rows.Select(MapTestimonial).ToList();
    }

    public async Task<int> CreateGalleryItemAsync(Guid gymId, CreateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@FileId", dto.FileId);
        parameters.Add("@Caption", dto.Caption);
        parameters.Add("@DisplayOrder", dto.DisplayOrder);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateGymWebsiteGalleryItem, parameters, "@Id", cancellationToken);
    }

    public Task UpdateGalleryItemAsync(Guid gymId, UpdateGymWebsiteGalleryItemDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateGymWebsiteGalleryItem, new
        {
            GymId = gymId,
            dto.Id,
            dto.Caption,
            dto.DisplayOrder
        }, cancellationToken);

    public Task DeleteGalleryItemAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteGymWebsiteGalleryItem, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<GymWebsiteGalleryItemDto>> GetGalleryAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymWebsiteGalleryRow>(StoredProcedureNames.GetGymWebsiteGallery, new { GymId = gymId }, cancellationToken);
        return rows.Select(MapGallery).ToList();
    }

    public async Task<int> CreateLeadCaptureAsync(Guid gymId, int? leadId, string name, string mobile, string? email, string source,
        string? interestedPlan, string? notes, string status, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@LeadId", leadId);
        parameters.Add("@Name", name);
        parameters.Add("@MobileNumber", mobile);
        parameters.Add("@Email", email);
        parameters.Add("@Source", source);
        parameters.Add("@InterestedPlan", interestedPlan);
        parameters.Add("@Notes", notes);
        parameters.Add("@Status", status);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateWebsiteLeadCapture, parameters, "@Id", cancellationToken);
    }

    public async Task<PagedResultDto<WebsiteLeadCaptureDto>> SearchLeadsAsync(Guid gymId, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@Status", query.Status);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<WebsiteLeadCaptureRow>(StoredProcedureNames.SearchWebsiteLeadCaptures, parameters, cancellationToken);
        return new PagedResultDto<WebsiteLeadCaptureDto>
        {
            Items = rows.Select(MapLeadCapture).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<WebsiteLeadCaptureDto>> SearchAllLeadsAsync(Guid gymId, WebsiteLeadSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var paged = await SearchLeadsAsync(gymId, new WebsiteLeadSearchQueryDto
        {
            Search = query.Search,
            Status = query.Status,
            PageNumber = 1,
            PageSize = 5000
        }, cancellationToken);
        return paged.Items;
    }

    public Task LinkLeadCaptureAsync(Guid gymId, int websiteLeadId, int leadId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ConvertWebsiteLeadCapture, new { GymId = gymId, WebsiteLeadId = websiteLeadId, LeadId = leadId }, cancellationToken);

    public async Task<PublicWebsiteDto?> GetPublicWebsiteAsync(string websiteSlug, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetPublicWebsiteBySlug,
            new { WebsiteSlug = websiteSlug },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var settingsRow = await multi.ReadSingleOrDefaultAsync<WebsiteSettingsRow>();
        if (settingsRow is null) return null;

        var sections = (await multi.ReadAsync<GymWebsiteSectionRow>()).Select(MapSection).ToList();
        var pages = (await multi.ReadAsync<GymWebsitePageRow>()).Select(MapPage).ToList();
        var testimonials = (await multi.ReadAsync<GymWebsiteTestimonialRow>()).Select(MapTestimonial).ToList();
        var gallery = (await multi.ReadAsync<GymWebsiteGalleryRow>()).Select(MapGallery).ToList();
        var plans = (await multi.ReadAsync<MembershipPlanRow>()).Select(DtoMapper.ToMembershipPlanDto).ToList();
        var trainers = (await multi.ReadAsync<PublicWebsiteTrainerRow>()).Select(r => new PublicWebsiteTrainerDto
        {
            Id = r.Id,
            FullName = r.FullName,
            Specialization = r.Specialization,
            Bio = r.Bio,
            ProfileImageUrl = r.ProfileImageUrl
        }).ToList();

        return new PublicWebsiteDto
        {
            Settings = MapSettings(settingsRow),
            GymName = settingsRow.GymName ?? string.Empty,
            Sections = sections,
            Pages = pages,
            Testimonials = testimonials,
            Gallery = gallery,
            MembershipPlans = plans,
            Trainers = trainers
        };
    }

    public async Task<Guid?> GetGymIdBySlugAsync(string websiteSlug, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymIdRow>(
            StoredProcedureNames.GetGymIdByWebsiteSlug, new { WebsiteSlug = websiteSlug }, cancellationToken);
        return row?.GymId;
    }

    public async Task<WebsiteAnalyticsOverviewDto> GetAnalyticsAsync(Guid gymId, int days, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetWebsiteAnalyticsOverview,
            new { GymId = gymId, Days = days },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleOrDefaultAsync<WebsiteAnalyticsSummaryRow>();
        var daily = (await multi.ReadAsync<WebsiteDailyLeadRow>()).Select(r => new WebsiteDailyLeadDto
        {
            LeadDate = r.LeadDate,
            LeadCount = r.LeadCount
        }).ToList();
        var sources = (await multi.ReadAsync<NamedCountRow>()).Select(r => new NamedCountDto
        {
            Name = r.Source ?? r.Name ?? string.Empty,
            Count = r.LeadCount != 0 ? r.LeadCount : r.Count
        }).ToList();

        return new WebsiteAnalyticsOverviewDto
        {
            TotalWebsiteLeads = summary?.TotalWebsiteLeads ?? 0,
            TrialRequests = summary?.TrialRequests ?? 0,
            ConvertedLeads = summary?.ConvertedLeads ?? 0,
            LeadConversionRate = summary?.LeadConversionRate ?? 0,
            LeadsInPeriod = summary?.LeadsInPeriod ?? 0,
            DailyLeads = daily,
            TopSources = sources
        };
    }

    public async Task<IReadOnlyList<WebsiteNotificationRecipientDto>> GetNotificationRecipientsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<WebsiteNotificationRecipientRow>(
            StoredProcedureNames.GetWebsiteNotificationRecipients, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new WebsiteNotificationRecipientDto
        {
            UserId = r.UserId,
            Name = r.Name,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            RecipientType = r.RecipientType
        }).ToList();
    }

    private static GymWebsiteSettingsDto MapSettings(WebsiteSettingsRow row) => new()
    {
        GymId = row.GymId,
        WebsiteSlug = row.WebsiteSlug,
        WebsiteTitle = row.WebsiteTitle,
        WebsiteDescription = row.WebsiteDescription,
        LogoFileId = row.LogoFileId,
        BannerFileId = row.BannerFileId,
        LogoUrl = row.LogoUrl,
        BannerUrl = row.BannerUrl,
        PrimaryColor = row.PrimaryColor,
        SecondaryColor = row.SecondaryColor,
        ContactPhone = row.ContactPhone,
        ContactEmail = row.ContactEmail,
        WhatsAppNumber = row.WhatsAppNumber,
        Address = row.Address,
        GoogleMapEmbedUrl = row.GoogleMapEmbedUrl,
        FacebookUrl = row.FacebookUrl,
        InstagramUrl = row.InstagramUrl,
        YoutubeUrl = row.YoutubeUrl,
        MetaTitle = row.MetaTitle,
        MetaDescription = row.MetaDescription,
        MetaKeywords = row.MetaKeywords,
        IsPublished = row.IsPublished,
        PublishedDate = row.PublishedDate
    };

    private static GymWebsitePageDto MapPage(GymWebsitePageRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        PageName = row.PageName,
        Slug = row.Slug,
        PageContent = row.PageContent,
        DisplayOrder = row.DisplayOrder,
        IsActive = row.IsActive
    };

    private static GymWebsiteSectionDto MapSection(GymWebsiteSectionRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        SectionType = row.SectionType,
        Title = row.Title,
        Subtitle = row.Subtitle,
        Description = row.Description,
        ImageFileId = row.ImageFileId,
        ImageUrl = row.ImageUrl,
        DisplayOrder = row.DisplayOrder,
        IsVisible = row.IsVisible
    };

    private static GymWebsiteTestimonialDto MapTestimonial(GymWebsiteTestimonialRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        MemberName = row.MemberName,
        Rating = row.Rating,
        ReviewText = row.ReviewText,
        ImageFileId = row.ImageFileId,
        ImageUrl = row.ImageUrl,
        IsApproved = row.IsApproved
    };

    private static GymWebsiteGalleryItemDto MapGallery(GymWebsiteGalleryRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        FileId = row.FileId,
        Caption = row.Caption,
        DisplayOrder = row.DisplayOrder,
        PublicUrl = row.PublicUrl,
        OriginalFileName = row.OriginalFileName
    };

    private static WebsiteLeadCaptureDto MapLeadCapture(WebsiteLeadCaptureRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        LeadId = row.LeadId,
        Name = row.Name,
        MobileNumber = row.MobileNumber,
        Email = row.Email,
        Source = row.Source,
        InterestedPlan = row.InterestedPlan,
        Notes = row.Notes,
        Status = row.Status,
        LeadStatus = row.LeadStatus,
        CreatedDate = row.CreatedDate
    };

    private sealed class WebsiteSettingsRow
    {
        public Guid GymId { get; set; }
        public string? GymName { get; set; }
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

    private sealed class GymWebsitePageRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? PageContent { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class GymWebsiteSectionRow
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

    private sealed class GymWebsiteTestimonialRow
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

    private sealed class GymWebsiteGalleryRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public long FileId { get; set; }
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public string? PublicUrl { get; set; }
        public string? OriginalFileName { get; set; }
    }

    private sealed class WebsiteLeadCaptureRow
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

    private sealed class PublicWebsiteTrainerRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    private sealed class GymIdRow
    {
        public Guid GymId { get; set; }
    }

    private sealed class WebsiteAnalyticsSummaryRow
    {
        public int TotalWebsiteLeads { get; set; }
        public int TrialRequests { get; set; }
        public int ConvertedLeads { get; set; }
        public decimal LeadConversionRate { get; set; }
        public int LeadsInPeriod { get; set; }
    }

    private sealed class WebsiteDailyLeadRow
    {
        public DateTime LeadDate { get; set; }
        public int LeadCount { get; set; }
    }

    private sealed class NamedCountRow
    {
        public string? Name { get; set; }
        public string? Source { get; set; }
        public int Count { get; set; }
        public int LeadCount { get; set; }
    }

    private sealed class WebsiteNotificationRecipientRow
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string RecipientType { get; set; } = string.Empty;
    }
}
