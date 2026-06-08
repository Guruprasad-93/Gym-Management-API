using System.Data;
using Dapper;
using Gym.Application.DTOs.WhiteLabel;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class WhiteLabelRepository : IWhiteLabelRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public WhiteLabelRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<int> UpsertSettingsAsync(Guid gymId, UpsertWhiteLabelSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BrandName", dto.BrandName.Trim());
        parameters.Add("@LogoFileId", dto.LogoFileId);
        parameters.Add("@FaviconFileId", dto.FaviconFileId);
        parameters.Add("@PrimaryColor", dto.PrimaryColor);
        parameters.Add("@SecondaryColor", dto.SecondaryColor);
        parameters.Add("@LoginBackgroundFileId", dto.LoginBackgroundFileId);
        parameters.Add("@AppDisplayName", dto.AppDisplayName);
        parameters.Add("@SupportEmail", dto.SupportEmail);
        parameters.Add("@SupportPhone", dto.SupportPhone);
        parameters.Add("@CustomDomain", dto.CustomDomain);
        parameters.Add("@SubDomain", dto.SubDomain);
        parameters.Add("@IsWhiteLabelEnabled", dto.IsWhiteLabelEnabled);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertWhiteLabelSettings, parameters, "@Id", cancellationToken);
    }

    public async Task<WhiteLabelSettingsDto?> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WhiteLabelSettingsRow>(
            StoredProcedureNames.GetWhiteLabelSettings, new { GymId = gymId }, cancellationToken);
        return row is null ? null : MapSettings(row);
    }

    public Task SetEnabledAsync(Guid gymId, bool enabled, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetWhiteLabelEnabled, new { GymId = gymId, IsWhiteLabelEnabled = enabled }, cancellationToken);

    public Task UpdateDomainAsync(Guid gymId, UpdateWhiteLabelDomainDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateWhiteLabelDomain, new { GymId = gymId, dto.SubDomain, dto.CustomDomain }, cancellationToken);

    public async Task<WhiteLabelSettingsDto?> GetBySubDomainAsync(string subDomain, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WhiteLabelSettingsRow>(
            StoredProcedureNames.GetWhiteLabelBySubDomain, new { SubDomain = subDomain }, cancellationToken);
        return row is null ? null : MapSettings(row);
    }

    public async Task<WhiteLabelLoginBrandingDto?> GetLoginBrandingAsync(WhiteLabelLoginBrandingQueryDto query, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WhiteLabelLoginBrandingRow>(
            StoredProcedureNames.GetWhiteLabelLoginBranding,
            new { query.GymId, query.SubDomain, query.CustomDomain }, cancellationToken);
        return row is null ? null : new WhiteLabelLoginBrandingDto
        {
            GymId = row.GymId,
            BrandName = row.BrandName,
            AppDisplayName = row.AppDisplayName,
            PrimaryColor = row.PrimaryColor,
            SecondaryColor = row.SecondaryColor,
            SupportEmail = row.SupportEmail,
            SupportPhone = row.SupportPhone,
            LogoUrl = row.LogoUrl,
            LoginBackgroundUrl = row.LoginBackgroundUrl
        };
    }

    public async Task<int> CreateEmailTemplateAsync(Guid gymId, UpsertWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TemplateName", dto.TemplateName);
        parameters.Add("@Subject", dto.Subject);
        parameters.Add("@Body", dto.Body);
        parameters.Add("@IsActive", dto.IsActive);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateWhiteLabelEmailTemplate, parameters, "@Id", cancellationToken);
    }

    public Task UpdateEmailTemplateAsync(Guid gymId, UpdateWhiteLabelEmailTemplateDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateWhiteLabelEmailTemplate, new
        {
            GymId = gymId,
            dto.Id,
            dto.TemplateName,
            dto.Subject,
            dto.Body,
            dto.IsActive
        }, cancellationToken);

    public Task DeleteEmailTemplateAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteWhiteLabelEmailTemplate, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<WhiteLabelEmailTemplateDto>> GetEmailTemplatesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<WhiteLabelEmailTemplateRow>(
            StoredProcedureNames.GetWhiteLabelEmailTemplates, new { GymId = gymId }, cancellationToken);
        return rows.Select(r => new WhiteLabelEmailTemplateDto
        {
            Id = r.Id,
            GymId = r.GymId,
            TemplateName = r.TemplateName,
            Subject = r.Subject,
            Body = r.Body,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<int> UpsertMobileSettingsAsync(Guid gymId, UpsertWhiteLabelMobileSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@AppName", dto.AppName);
        parameters.Add("@SplashScreenFileId", dto.SplashScreenFileId);
        parameters.Add("@AppIconFileId", dto.AppIconFileId);
        parameters.Add("@AndroidPackageName", dto.AndroidPackageName);
        parameters.Add("@IOSBundleId", dto.IOSBundleId);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertWhiteLabelMobileSettings, parameters, "@Id", cancellationToken);
    }

    public async Task<WhiteLabelMobileSettingsDto?> GetMobileSettingsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WhiteLabelMobileSettingsRow>(
            StoredProcedureNames.GetWhiteLabelMobileSettings, new { GymId = gymId }, cancellationToken);
        return row is null ? null : MapMobile(row);
    }

    public async Task<WhiteLabelPlatformDashboardDto> GetPlatformDashboardAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetWhiteLabelPlatformDashboard,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleOrDefaultAsync<PlatformSummaryRow>();
        var customers = (await multi.ReadAsync<CustomerSummaryRow>()).Select(r => new WhiteLabelCustomerSummaryDto
        {
            BrandName = r.BrandName,
            SubDomain = r.SubDomain,
            CustomDomain = r.CustomDomain,
            SubscriptionStatus = r.Status,
            CurrentPeriodEnd = r.CurrentPeriodEnd
        }).ToList();
        var trend = (await multi.ReadAsync<AdoptionRow>()).Select(r => new WhiteLabelAdoptionPointDto
        {
            AdoptionDate = r.AdoptionDate,
            EnabledCount = r.EnabledCount
        }).ToList();

        return new WhiteLabelPlatformDashboardDto
        {
            TotalWhiteLabelCustomers = summary?.TotalWhiteLabelCustomers ?? 0,
            SubDomainCustomers = summary?.SubDomainCustomers ?? 0,
            CustomDomainCustomers = summary?.CustomDomainCustomers ?? 0,
            WhiteLabelMonthlyRevenue = summary?.WhiteLabelMonthlyRevenue ?? 0,
            ExpiringWhiteLabelPlans = summary?.ExpiringWhiteLabelPlans ?? 0,
            PremiumCustomers = customers,
            AdoptionTrend = trend
        };
    }

    private static WhiteLabelSettingsDto MapSettings(WhiteLabelSettingsRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        BrandName = row.BrandName,
        LogoFileId = row.LogoFileId,
        FaviconFileId = row.FaviconFileId,
        LogoUrl = row.LogoUrl,
        FaviconUrl = row.FaviconUrl,
        PrimaryColor = row.PrimaryColor,
        SecondaryColor = row.SecondaryColor,
        LoginBackgroundFileId = row.LoginBackgroundFileId,
        LoginBackgroundUrl = row.LoginBackgroundUrl,
        AppDisplayName = row.AppDisplayName,
        SupportEmail = row.SupportEmail,
        SupportPhone = row.SupportPhone,
        CustomDomain = row.CustomDomain,
        SubDomain = row.SubDomain,
        IsWhiteLabelEnabled = row.IsWhiteLabelEnabled,
        CreatedAt = row.CreatedAt,
        UpdatedAt = row.UpdatedAt
    };

    private static WhiteLabelMobileSettingsDto MapMobile(WhiteLabelMobileSettingsRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        AppName = row.AppName,
        SplashScreenFileId = row.SplashScreenFileId,
        AppIconFileId = row.AppIconFileId,
        SplashScreenUrl = row.SplashScreenUrl,
        AppIconUrl = row.AppIconUrl,
        AndroidPackageName = row.AndroidPackageName,
        IOSBundleId = row.IOSBundleId
    };

    private sealed class WhiteLabelSettingsRow
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

    private sealed class WhiteLabelLoginBrandingRow
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
    }

    private sealed class WhiteLabelEmailTemplateRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class WhiteLabelMobileSettingsRow
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

    private sealed class PlatformSummaryRow
    {
        public int TotalWhiteLabelCustomers { get; set; }
        public int SubDomainCustomers { get; set; }
        public int CustomDomainCustomers { get; set; }
        public decimal WhiteLabelMonthlyRevenue { get; set; }
        public int ExpiringWhiteLabelPlans { get; set; }
    }

    private sealed class CustomerSummaryRow
    {
        public string BrandName { get; set; } = string.Empty;
        public string? SubDomain { get; set; }
        public string? CustomDomain { get; set; }
        public string? Status { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
    }

    private sealed class AdoptionRow
    {
        public DateTime AdoptionDate { get; set; }
        public int EnabledCount { get; set; }
    }
}