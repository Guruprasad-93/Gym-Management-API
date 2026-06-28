using System.Data;
using Dapper;
using Gym.Application.DTOs.Saas;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class PlanManagementRepository : IPlanManagementRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public PlanManagementRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PlanSummaryDto>> GetPlatformPlanSummariesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PlanSummaryRow>(
            StoredProcedureNames.SaasPlatformListPlans,
            cancellationToken: cancellationToken);

        return rows.Select(r => new PlanSummaryDto
        {
            Id = r.SaasPlanId,
            PlanCode = r.PlanCode,
            PlanName = r.PlanName,
            Description = r.Description,
            IsTrialPlan = r.IsTrialPlan,
            IsPublic = r.IsPublic,
            TrialDays = r.TrialDays,
            IsActive = r.IsActive,
            SortOrder = r.SortOrder,
            CreatedAt = r.CreatedAt,
            ActiveSubscriberCount = r.ActiveSubscriberCount,
            FeatureCount = r.FeatureCount,
            PricingOptionCount = r.PricingOptionCount,
            Quotas = new PlanQuotaDto
            {
                SaasPlanId = r.SaasPlanId,
                MaxMembers = r.MaxMembers,
                MaxTrainers = r.MaxTrainers,
                MaxBranches = r.MaxBranches,
                MaxStorageGB = r.MaxStorageGB,
                MaxSmsPerMonth = r.MaxSmsPerMonth,
                MaxWhatsappMessages = r.MaxWhatsappMessages
            }
        }).ToList();
    }

    public async Task<IReadOnlyList<SaasPlanDto>> GetPlatformPlansAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<SaasPlanRow>(
            StoredProcedureNames.SaasGetAllPlans,
            new { IncludeInactive = true, PublicOnly = false },
            cancellationToken);

        return rows.Select(MapPlan).ToList();
    }

    public async Task<DynamicSaasPlanDto?> GetPlanDetailAsync(int saasPlanId, CancellationToken cancellationToken = default)
    {
        var planRow = await _sp.QuerySingleOrDefaultAsync<SaasPlanRow>(
            StoredProcedureNames.SaasGetPlanById,
            new { SaasPlanId = saasPlanId },
            cancellationToken);

        if (planRow is null)
            return null;

        var quotaRow = await _sp.QuerySingleOrDefaultAsync<PlanQuotaRow>(
            StoredProcedureNames.PlanQuotaGetByPlanId,
            new { SaasPlanId = saasPlanId },
            cancellationToken);

        var featureRows = await _sp.QueryAsync<PlanFeatureRow>(
            StoredProcedureNames.PlanFeatureGetByPlanId,
            new { SaasPlanId = saasPlanId },
            cancellationToken);

        var pricingRows = await _sp.QueryAsync<PlanPricingRow>(
            StoredProcedureNames.PlanPricingGetByPlanId,
            new { SaasPlanId = saasPlanId, IncludeInactive = true },
            cancellationToken);

        return MapDynamicPlan(planRow, quotaRow, featureRows, pricingRows);
    }

    public async Task<int> CreatePlanAsync(CreateDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PlanCode", dto.PlanCode);
        parameters.Add("@PlanName", dto.PlanName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@IsTrialPlan", dto.IsTrialPlan);
        parameters.Add("@IsPublic", dto.IsPublic);
        parameters.Add("@TrialDays", dto.TrialDays);
        parameters.Add("@SortOrder", dto.SortOrder);
        parameters.Add("@SaasPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteAsync(StoredProcedureNames.SaasPlatformCreatePlan, parameters, cancellationToken);
        var planId = parameters.Get<int>("@SaasPlanId");

        if (dto.Quotas is not null)
            await UpsertQuotasAsync(planId, dto.Quotas, cancellationToken);

        if (dto.FeatureIds.Count > 0)
            await SetPlanFeaturesAsync(planId, dto.FeatureIds, cancellationToken);

        return planId;
    }

    public async Task<int> ClonePlanAsync(int sourceSaasPlanId, CloneDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@SourceSaasPlanId", sourceSaasPlanId);
        parameters.Add("@PlanCode", dto.PlanCode);
        parameters.Add("@PlanName", dto.PlanName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@IsTrialPlan", dto.IsTrialPlan);
        parameters.Add("@IsPublic", dto.IsPublic);
        parameters.Add("@SortOrder", dto.SortOrder);
        parameters.Add("@NewSaasPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteAsync(StoredProcedureNames.SaasPlatformClonePlan, parameters, cancellationToken);
        return parameters.Get<int>("@NewSaasPlanId");
    }

    public async Task UpdatePlanAsync(int saasPlanId, UpdateDynamicPlanDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.SaasPlatformUpdatePlan, new
        {
            SaasPlanId = saasPlanId,
            dto.PlanCode,
            dto.PlanName,
            dto.Description,
            dto.IsTrialPlan,
            dto.IsPublic,
            dto.TrialDays,
            dto.SortOrder,
            dto.IsActive
        }, cancellationToken);

        if (dto.Quotas is not null)
            await UpsertQuotasAsync(saasPlanId, dto.Quotas, cancellationToken);

        await SetPlanFeaturesAsync(saasPlanId, dto.FeatureIds, cancellationToken);
    }

    public Task DeletePlanAsync(int saasPlanId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SaasPlatformDeletePlan, new { SaasPlanId = saasPlanId }, cancellationToken);

    public Task UpsertQuotasAsync(int saasPlanId, UpsertPlanQuotaDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.PlanQuotaUpsert, new
        {
            SaasPlanId = saasPlanId,
            dto.MaxMembers,
            dto.MaxTrainers,
            dto.MaxBranches,
            dto.MaxStorageGB,
            dto.MaxSmsPerMonth,
            dto.MaxWhatsappMessages
        }, cancellationToken);

    public Task SetPlanFeaturesAsync(int saasPlanId, IReadOnlyList<int> featureIds, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.PlanFeatureSetForPlan, new
        {
            SaasPlanId = saasPlanId,
            FeatureIds = string.Join(',', featureIds)
        }, cancellationToken);

    public async Task<int> CreatePricingOptionAsync(int saasPlanId, CreatePlanPricingOptionDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@SaasPlanId", saasPlanId);
        parameters.Add("@DurationValue", dto.DurationValue);
        parameters.Add("@DurationUnit", dto.DurationUnit);
        parameters.Add("@Price", dto.Price);
        parameters.Add("@DisplayLabel", dto.DisplayLabel);
        parameters.Add("@SortOrder", dto.SortOrder);
        parameters.Add("@PricingOptionId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteAsync(StoredProcedureNames.PlanPricingCreate, parameters, cancellationToken);
        return parameters.Get<int>("@PricingOptionId");
    }

    public Task UpdatePricingOptionAsync(int pricingOptionId, UpdatePlanPricingOptionDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.PlanPricingUpdate, new
        {
            PricingOptionId = pricingOptionId,
            dto.DurationValue,
            dto.DurationUnit,
            dto.Price,
            dto.DisplayLabel,
            dto.SortOrder,
            dto.IsActive
        }, cancellationToken);

    public Task DeletePricingOptionAsync(int pricingOptionId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.PlanPricingDelete, new { PricingOptionId = pricingOptionId }, cancellationToken);

    public Task ReorderPricingOptionsAsync(int saasPlanId, IReadOnlyList<PricingOptionOrderDto> items, CancellationToken cancellationToken = default)
    {
        var orders = string.Join(',', items.Select(i => $"{i.PricingOptionId}:{i.SortOrder}"));
        return _sp.ExecuteAsync(StoredProcedureNames.PlanPricingReorder, new
        {
            SaasPlanId = saasPlanId,
            PricingOptionOrders = orders
        }, cancellationToken);
    }

    public async Task<PlanPricingOptionDto?> GetPricingOptionByIdAsync(int pricingOptionId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PlanPricingRow>(
            StoredProcedureNames.PlanPricingGetById,
            new { PricingOptionId = pricingOptionId },
            cancellationToken);

        return row is null ? null : MapPricing(row);
    }

    public async Task<SaasPlanCatalogDto> GetPlanCatalogAsync(bool publicOnly = true, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.SaasGetPlanCatalog,
                new { PublicOnly = publicOnly },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var planRows = (await multi.ReadAsync<SaasCatalogPlanRow>()).ToList();
        var pricingRows = (await multi.ReadAsync<PlanPricingRow>()).ToList();
        var featureRows = (await multi.ReadAsync<PlanFeatureRow>()).ToList();

        var pricingByPlan = pricingRows.GroupBy(p => p.SaasPlanId).ToDictionary(g => g.Key, g => g.ToList());
        var featuresByPlan = featureRows.GroupBy(f => f.SaasPlanId).ToDictionary(g => g.Key, g => g.ToList());

        var plans = planRows.Select(row =>
        {
            pricingByPlan.TryGetValue(row.SaasPlanId, out var pricing);
            featuresByPlan.TryGetValue(row.SaasPlanId, out var features);

            return new SaasPlanCatalogItemDto
            {
                Id = row.SaasPlanId,
                PlanCode = row.PlanCode,
                PlanName = row.PlanName,
                Description = row.Description,
                SortOrder = row.SortOrder,
                Quotas = new PlanQuotaDto
                {
                    SaasPlanId = row.SaasPlanId,
                    MaxMembers = row.MaxMembers,
                    MaxTrainers = row.MaxTrainers,
                    MaxBranches = row.MaxBranches,
                    MaxStorageGB = row.MaxStorageGB,
                    MaxSmsPerMonth = row.MaxSmsPerMonth,
                    MaxWhatsappMessages = row.MaxWhatsappMessages
                },
                PricingOptions = (pricing ?? []).Select(MapCatalogPricing).ToList(),
                Features = (features ?? []).Select(MapFeature).ToList()
            };
        }).ToList();

        return new SaasPlanCatalogDto { Plans = plans };
    }

    private static SaasPlanDto MapPlan(SaasPlanRow row) => new()
    {
        Id = row.SaasPlanId,
        PlanCode = row.PlanCode,
        PlanName = row.PlanName,
        Description = row.Description,
        IsTrialPlan = row.IsTrialPlan,
        IsPublic = row.IsPublic,
        MaxMembers = row.MaxMembers,
        MaxTrainers = row.MaxTrainers,
        MaxBranches = row.MaxBranches,
        MaxStorageGB = row.MaxStorageGB,
        MaxSmsPerMonth = row.MaxSmsPerMonth,
        MaxWhatsappMessages = row.MaxWhatsappMessages,
        StorageLimitMb = row.StorageLimitMb,
        WhatsAppNotificationLimit = row.WhatsAppNotificationLimit,
        MonthlyPrice = row.MonthlyPrice,
        QuarterlyPrice = row.QuarterlyPrice,
        HalfYearlyPrice = row.HalfYearlyPrice,
        YearlyPrice = row.YearlyPrice,
        TrialDays = row.TrialDays,
        IsActive = row.IsActive,
        SortOrder = row.SortOrder
    };

    private static DynamicSaasPlanDto MapDynamicPlan(
        SaasPlanRow plan,
        PlanQuotaRow? quota,
        IReadOnlyList<PlanFeatureRow> features,
        IReadOnlyList<PlanPricingRow> pricing) => new()
    {
        Id = plan.SaasPlanId,
        PlanCode = plan.PlanCode,
        PlanName = plan.PlanName,
        Description = plan.Description,
        IsTrialPlan = plan.IsTrialPlan,
        IsPublic = plan.IsPublic,
        TrialDays = plan.TrialDays,
        IsActive = plan.IsActive,
        SortOrder = plan.SortOrder,
        ActiveSubscriberCount = 0,
        FeatureCount = features.Count,
        PricingOptionCount = pricing.Count,
        MonthlyPrice = plan.MonthlyPrice,
        QuarterlyPrice = plan.QuarterlyPrice,
        HalfYearlyPrice = plan.HalfYearlyPrice,
        YearlyPrice = plan.YearlyPrice,
        Quotas = quota is null ? null : new PlanQuotaDto
        {
            PlanQuotaId = quota.PlanQuotaId,
            SaasPlanId = quota.SaasPlanId,
            MaxMembers = quota.MaxMembers,
            MaxTrainers = quota.MaxTrainers,
            MaxBranches = quota.MaxBranches,
            MaxStorageGB = quota.MaxStorageGB,
            MaxSmsPerMonth = quota.MaxSmsPerMonth,
            MaxWhatsappMessages = quota.MaxWhatsappMessages,
            StorageLimitMb = quota.StorageLimitMb,
            WhatsAppNotificationLimit = quota.WhatsAppNotificationLimit
        },
        Features = features.Select(MapFeature).ToList(),
        PricingOptions = pricing.Select(MapPricing).ToList()
    };

    private static PlanFeatureAssignmentDto MapFeature(PlanFeatureRow row) => new()
    {
        PlanFeatureId = row.PlanFeatureId,
        SaasPlanId = row.SaasPlanId,
        FeatureId = row.FeatureId,
        FeatureCode = row.FeatureCode,
        FeatureName = row.FeatureName,
        Category = row.Category,
        IsIncluded = row.IsIncluded
    };

    private static PlanPricingOptionDto MapPricing(PlanPricingRow row) => new()
    {
        PricingOptionId = row.PricingOptionId,
        SaasPlanId = row.SaasPlanId,
        DurationValue = row.DurationValue,
        DurationUnit = row.DurationUnit,
        Price = row.Price,
        Currency = row.Currency,
        DisplayLabel = row.DisplayLabel,
        IsActive = row.IsActive,
        SortOrder = row.SortOrder
    };

    /// <summary>
    /// Catalog SP filters to active pricing rows; treat missing IsActive as active for legacy SP versions.
    /// </summary>
    private static PlanPricingOptionDto MapCatalogPricing(PlanPricingRow row)
    {
        var dto = MapPricing(row);
        if (!dto.IsActive && row.Price >= 0)
            dto.IsActive = true;
        return dto;
    }
}
