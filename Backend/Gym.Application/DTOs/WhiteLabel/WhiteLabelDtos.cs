namespace Gym.Application.DTOs.WhiteLabel;

public sealed class WhiteLabelSettingsDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public long? LogoFileId { get; set; }
    public long? FaviconFileId { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public long? LoginBackgroundFileId { get; set; }
    public string? LoginBackgroundUrl { get; set; }
    public string? AppDisplayName { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? CustomDomain { get; set; }
    public string? SubDomain { get; set; }
    public bool IsWhiteLabelEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertWhiteLabelSettingsDto
{
    public string BrandName { get; set; } = string.Empty;
    public long? LogoFileId { get; set; }
    public long? FaviconFileId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public long? LoginBackgroundFileId { get; set; }
    public string? AppDisplayName { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? CustomDomain { get; set; }
    public string? SubDomain { get; set; }
    public bool IsWhiteLabelEnabled { get; set; }
}

public sealed class UpdateWhiteLabelDomainDto
{
    public string? SubDomain { get; set; }
    public string? CustomDomain { get; set; }
}

public sealed class WhiteLabelEmailTemplateDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpsertWhiteLabelEmailTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateWhiteLabelEmailTemplateDto : UpsertWhiteLabelEmailTemplateDto
{
    public int Id { get; set; }
}

public sealed class WhiteLabelMobileSettingsDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public string? AppName { get; set; }
    public long? SplashScreenFileId { get; set; }
    public long? AppIconFileId { get; set; }
    public string? SplashScreenUrl { get; set; }
    public string? AppIconUrl { get; set; }
    public string? AndroidPackageName { get; set; }
    public string? IOSBundleId { get; set; }
}

public class UpsertWhiteLabelMobileSettingsDto
{
    public string? AppName { get; set; }
    public long? SplashScreenFileId { get; set; }
    public long? AppIconFileId { get; set; }
    public string? AndroidPackageName { get; set; }
    public string? IOSBundleId { get; set; }
}

public sealed class WhiteLabelLoginBrandingDto
{
    public Guid GymId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? AppDisplayName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? LogoUrl { get; set; }
    public string? LoginBackgroundUrl { get; set; }
    /// <summary>When true, UI shows "Powered by [platform]" footer (Basic plan).</summary>
    public bool ShowPoweredBy { get; set; }
    public string? PlatformProductName { get; set; }
}

public sealed class WhiteLabelPreviewDto
{
    public WhiteLabelLoginBrandingDto Login { get; set; } = new();
    public WhiteLabelWebsitePreviewDto Website { get; set; } = new();
    public WhiteLabelMobileSettingsDto Mobile { get; set; } = new();
}

public sealed class WhiteLabelWebsitePreviewDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? WebsiteTitle { get; set; }
}

public sealed class WhiteLabelPlatformDashboardDto
{
    public int TotalWhiteLabelCustomers { get; set; }
    public int SubDomainCustomers { get; set; }
    public int CustomDomainCustomers { get; set; }
    public decimal WhiteLabelMonthlyRevenue { get; set; }
    public int ExpiringWhiteLabelPlans { get; set; }
    public IReadOnlyList<WhiteLabelCustomerSummaryDto> PremiumCustomers { get; set; } = [];
    public IReadOnlyList<WhiteLabelAdoptionPointDto> AdoptionTrend { get; set; } = [];
}

public sealed class WhiteLabelCustomerSummaryDto
{
    public string BrandName { get; set; } = string.Empty;
    public string? SubDomain { get; set; }
    public string? CustomDomain { get; set; }
    public string? SubscriptionStatus { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
}

public sealed class WhiteLabelAdoptionPointDto
{
    public DateTime AdoptionDate { get; set; }
    public int EnabledCount { get; set; }
}

public sealed class WhiteLabelLoginBrandingQueryDto
{
    public Guid? GymId { get; set; }
    public string? SubDomain { get; set; }
    public string? CustomDomain { get; set; }
}
