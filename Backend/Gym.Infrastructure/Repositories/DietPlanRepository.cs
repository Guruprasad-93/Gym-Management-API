using System.Data;
using System.Text.Json;
using Dapper;
using Gym.Application.DTOs.DietPlans;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class DietPlanRepository : IDietPlanRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public DietPlanRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public Task SeedCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SeedDietCategories, new { GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<DietCategoryDto>> GetCategoriesAsync(
        Guid gymId, bool includeInactive, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<DietCategoryDto>(
            StoredProcedureNames.GetDietCategories,
            new { GymId = gymId, IncludeInactive = includeInactive },
            cancellationToken)).ToList();

    public async Task<DietCategoryDto> CreateCategoryAsync(
        Guid gymId, CreateDietCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@CategoryName", dto.CategoryName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@DietCategoryId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateDietCategory, parameters, "@DietCategoryId", cancellationToken);

        return (await GetCategoriesAsync(gymId, true, cancellationToken)).First(c => c.DietCategoryId == id);
    }

    public async Task<IReadOnlyList<DietPlanListDto>> GetPlansAsync(
        Guid? gymId, bool includeInactive, int? categoryId, string? search, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<DietPlanListDto>(
            StoredProcedureNames.GetDietPlans,
            new { GymId = gymId, IncludeInactive = includeInactive, CategoryId = categoryId, Search = search },
            cancellationToken)).ToList();

    public async Task<DietPlanDetailDto?> GetPlanByIdAsync(
        int dietPlanId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.GetDietPlanById,
                new { DietPlanId = dietPlanId, GymId = gymId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var plan = await multi.ReadSingleOrDefaultAsync<DietPlanDetailDto>();
        if (plan is null)
            return null;

        var items = (await multi.ReadAsync<DietPlanItemDto>()).ToList();
        plan.Items = items;
        return plan;
    }

    public async Task<int> CreatePlanAsync(
        Guid gymId, CreateDietPlanDto dto, Guid? createdByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@PlanName", dto.PlanName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@DietCategoryId", dto.DietCategoryId);
        parameters.Add("@TargetCalories", dto.TargetCalories);
        parameters.Add("@IsActive", dto.IsActive);
        parameters.Add("@CreatedByUserId", createdByUserId);
        parameters.Add("@DietPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var planId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateDietPlan, parameters, "@DietPlanId", cancellationToken);

        await ReplaceItemsAsync(planId, gymId, dto.Items, cancellationToken);
        return planId;
    }

    public async Task UpdatePlanAsync(
        int dietPlanId, Guid gymId, UpdateDietPlanDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.UpdateDietPlan, new
        {
            DietPlanId = dietPlanId,
            GymId = gymId,
            dto.PlanName,
            dto.Description,
            dto.DietCategoryId,
            dto.TargetCalories,
            dto.IsActive
        }, cancellationToken);

        await ReplaceItemsAsync(dietPlanId, gymId, dto.Items, cancellationToken);
    }

    public Task DeletePlanAsync(int dietPlanId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteDietPlan, new { DietPlanId = dietPlanId, GymId = gymId }, cancellationToken);

    public Task SetActiveAsync(int dietPlanId, Guid gymId, bool isActive, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetDietPlanActive, new { DietPlanId = dietPlanId, GymId = gymId, IsActive = isActive }, cancellationToken);

    public async Task<int> ClonePlanAsync(
        int sourcePlanId, Guid gymId, string? newPlanName, Guid? createdByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@SourceDietPlanId", sourcePlanId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@NewPlanName", newPlanName);
        parameters.Add("@CreatedByUserId", createdByUserId);
        parameters.Add("@NewDietPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        return await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CloneDietPlan, parameters, "@NewDietPlanId", cancellationToken);
    }

    public async Task<int> AssignToMemberAsync(
        Guid gymId, AssignDietPlanDto dto, Guid? assignedByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@DietPlanId", dto.DietPlanId);
        parameters.Add("@AssignedByUserId", assignedByUserId);
        parameters.Add("@StartDate", dto.StartDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@EndDate", dto.EndDate?.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@DeactivatePrevious", dto.DeactivatePrevious);
        parameters.Add("@AssignedDietPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        return await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.AssignDietPlanToMember, parameters, "@AssignedDietPlanId", cancellationToken);
    }

    public Task UnassignAsync(int assignedDietPlanId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UnassignDietPlan, new { AssignedDietPlanId = assignedDietPlanId, GymId = gymId }, cancellationToken);

    public async Task<MemberDietPlanViewDto> GetMemberDietAsync(
        int memberId, Guid? gymId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.GetMemberAssignedDietPlan,
                new { MemberId = memberId, GymId = gymId, ActiveOnly = activeOnly },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var header = await multi.ReadSingleOrDefaultAsync<MemberDietPlanHeaderRow>();
        var items = (await multi.ReadAsync<DietPlanItemDto>()).ToList();

        if (header is null)
            return new MemberDietPlanViewDto { MemberId = memberId, Items = items };

        return new MemberDietPlanViewDto
        {
            AssignedDietPlanId = header.AssignedDietPlanId,
            MemberId = header.MemberId,
            MemberName = header.MemberName,
            DietPlanId = header.DietPlanId,
            PlanName = header.PlanName,
            PlanDescription = header.PlanDescription,
            TargetCalories = header.TargetCalories,
            CategoryName = header.CategoryName,
            StartDate = header.StartDate,
            EndDate = header.EndDate,
            AssignmentNotes = header.AssignmentNotes,
            IsActive = header.IsActive,
            Items = items
        };
    }

    public async Task<IReadOnlyList<MemberDietAssignmentDto>> GetMemberAssignmentsAsync(
        int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<MemberDietAssignmentDto>(
            StoredProcedureNames.GetMemberDietAssignments,
            new { MemberId = memberId, GymId = gymId },
            cancellationToken)).ToList();

    private Task ReplaceItemsAsync(
        int dietPlanId, Guid gymId, IReadOnlyList<DietPlanItemInputDto> items, CancellationToken cancellationToken)
    {
        var json = items.Count == 0 ? null : JsonSerializer.Serialize(items, JsonOptions);
        return _sp.ExecuteAsync(StoredProcedureNames.ReplaceDietPlanItems, new
        {
            DietPlanId = dietPlanId,
            GymId = gymId,
            ItemsJson = json
        }, cancellationToken);
    }

    private sealed class MemberDietPlanHeaderRow
    {
        public int AssignedDietPlanId { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public int DietPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string? PlanDescription { get; set; }
        public int? TargetCalories { get; set; }
        public string? CategoryName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? AssignmentNotes { get; set; }
        public bool IsActive { get; set; }
    }
}
