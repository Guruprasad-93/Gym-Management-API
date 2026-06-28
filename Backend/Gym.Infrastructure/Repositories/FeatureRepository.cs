using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class FeatureRepository : IFeatureRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public FeatureRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<IReadOnlyList<SystemFeatureDto>> GetAllFeaturesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<SystemFeatureRow>(
            StoredProcedureNames.FeatureGetAll,
            new { IncludeInactive = includeInactive },
            cancellationToken);

        return rows.Select(r => new SystemFeatureDto
        {
            FeatureId = r.FeatureId,
            FeatureCode = r.FeatureCode,
            FeatureName = r.FeatureName,
            Description = r.Description,
            Category = r.Category,
            MenuRoute = r.MenuRoute,
            MenuIcon = r.MenuIcon,
            IsMenuFeature = r.IsMenuFeature,
            IsApiFeature = r.IsApiFeature,
            IsQuotaFeature = r.IsQuotaFeature,
            SortOrder = r.SortOrder,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<IReadOnlyList<string>> GetEnabledFeatureCodesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<FeatureCodeRow>(
            StoredProcedureNames.GymGetEnabledFeatureCodes,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(r => r.FeatureCode).ToList();
    }

    public async Task<IReadOnlyList<string>> GetVisibleMenuCodesAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MenuCodeRow>(
            StoredProcedureNames.GymGetVisibleMenuCodes,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(r => r.MenuCode).ToList();
    }

    public async Task<IReadOnlyList<string>> GetMenuCodesForFeatureAsync(string featureCode, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MenuCodeRow>(
            StoredProcedureNames.FeatureGetMenuCodes,
            new { FeatureCode = featureCode },
            cancellationToken);

        return rows.Select(r => r.MenuCode).ToList();
    }

    public async Task<IReadOnlyList<FeatureApiRouteDto>> GetApiRoutesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<FeatureApiRouteRow>(
            StoredProcedureNames.FeatureGetApiRoutes,
            cancellationToken: cancellationToken);

        return rows.Select(r => new FeatureApiRouteDto
        {
            FeatureCode = r.FeatureCode,
            RoutePrefix = r.RoutePrefix,
            HttpMethods = r.HttpMethods
        }).ToList();
    }

    public async Task<IReadOnlyList<FeatureDependencyDto>> GetFeatureDependenciesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<FeatureDependencyRow>(
            StoredProcedureNames.FeatureGetDependencies,
            cancellationToken: cancellationToken);

        return rows.Select(r => new FeatureDependencyDto
        {
            FeatureCode = r.FeatureCode,
            RequiresFeatureCode = r.RequiresFeatureCode
        }).ToList();
    }

    private sealed class FeatureCodeRow
    {
        public string FeatureCode { get; set; } = string.Empty;
    }

    private sealed class MenuCodeRow
    {
        public string MenuCode { get; set; } = string.Empty;
    }

    private sealed class FeatureDependencyRow
    {
        public string FeatureCode { get; set; } = string.Empty;
        public string RequiresFeatureCode { get; set; } = string.Empty;
    }
}
