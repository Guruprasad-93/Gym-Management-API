using System.Data;
using System.Text.Json;
using Dapper;
using Gym.Application.DTOs.WorkoutPlans;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class WorkoutPlanRepository : IWorkoutPlanRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public WorkoutPlanRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public Task SeedExerciseCategoriesAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SeedExerciseCategories, new { GymId = gymId }, cancellationToken);

    public Task SeedExerciseLibraryAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SeedExerciseLibrary, new { GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<ExerciseCategoryDto>> GetCategoriesAsync(
        Guid gymId, bool includeInactive, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<ExerciseCategoryDto>(StoredProcedureNames.GetExerciseCategories,
            new { GymId = gymId, IncludeInactive = includeInactive }, cancellationToken)).ToList();

    public async Task<ExerciseCategoryDto> CreateCategoryAsync(
        Guid gymId, CreateExerciseCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@CategoryName", dto.CategoryName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@ExerciseCategoryId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var id = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateExerciseCategory, parameters, "@ExerciseCategoryId", cancellationToken);
        return (await GetCategoriesAsync(gymId, true, cancellationToken)).First(c => c.ExerciseCategoryId == id);
    }

    public async Task<IReadOnlyList<ExerciseDto>> GetExercisesAsync(
        Guid? gymId, bool includeInactive, int? categoryId, string? muscleGroup, string? search, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<ExerciseDto>(StoredProcedureNames.GetExerciseLibrary,
            new { GymId = gymId, IncludeInactive = includeInactive, CategoryId = categoryId, MuscleGroup = muscleGroup, Search = search },
            cancellationToken)).ToList();

    public Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId, Guid? gymId, CancellationToken cancellationToken = default) =>
        _sp.QuerySingleOrDefaultAsync<ExerciseDto>(StoredProcedureNames.GetExerciseById,
            new { ExerciseId = exerciseId, GymId = gymId }, cancellationToken);

    public async Task<int> CreateExerciseAsync(Guid gymId, CreateExerciseDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@ExerciseName", dto.ExerciseName);
        parameters.Add("@ExerciseCategoryId", dto.ExerciseCategoryId);
        parameters.Add("@MuscleGroup", dto.MuscleGroup);
        parameters.Add("@Difficulty", dto.Difficulty);
        parameters.Add("@Instructions", dto.Instructions);
        parameters.Add("@IsActive", dto.IsActive);
        parameters.Add("@ExerciseId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateExercise, parameters, "@ExerciseId", cancellationToken);
    }

    public Task UpdateExerciseAsync(int exerciseId, Guid gymId, UpdateExerciseDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateExercise, new
        {
            ExerciseId = exerciseId,
            GymId = gymId,
            dto.ExerciseName,
            dto.ExerciseCategoryId,
            dto.MuscleGroup,
            dto.Difficulty,
            dto.Instructions,
            dto.IsActive
        }, cancellationToken);

    public Task DeleteExerciseAsync(int exerciseId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteExercise, new { ExerciseId = exerciseId, GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<WorkoutPlanListDto>> GetPlansAsync(
        Guid? gymId, bool includeInactive, string? search, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<WorkoutPlanListDto>(StoredProcedureNames.GetWorkoutPlans,
            new { GymId = gymId, IncludeInactive = includeInactive, Search = search }, cancellationToken)).ToList();

    public async Task<WorkoutPlanDetailDto?> GetPlanByIdAsync(int workoutPlanId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetWorkoutPlanById,
            new { WorkoutPlanId = workoutPlanId, GymId = gymId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var plan = await multi.ReadSingleOrDefaultAsync<WorkoutPlanDetailDto>();
        if (plan is null) return null;
        plan.Exercises = (await multi.ReadAsync<WorkoutPlanExerciseDto>()).ToList();
        return plan;
    }

    public async Task<int> CreatePlanAsync(Guid gymId, CreateWorkoutPlanDto dto, Guid? createdByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@PlanName", dto.PlanName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@Goal", dto.Goal);
        parameters.Add("@DurationWeeks", dto.DurationWeeks);
        parameters.Add("@IsActive", dto.IsActive);
        parameters.Add("@CreatedByUserId", createdByUserId);
        parameters.Add("@WorkoutPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var planId = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateWorkoutPlan, parameters, "@WorkoutPlanId", cancellationToken);
        await ReplaceExercisesAsync(planId, gymId, dto.Exercises, cancellationToken);
        return planId;
    }

    public async Task UpdatePlanAsync(int workoutPlanId, Guid gymId, UpdateWorkoutPlanDto dto, CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(StoredProcedureNames.UpdateWorkoutPlan, new
        {
            WorkoutPlanId = workoutPlanId,
            GymId = gymId,
            dto.PlanName,
            dto.Description,
            dto.Goal,
            dto.DurationWeeks,
            dto.IsActive
        }, cancellationToken);
        await ReplaceExercisesAsync(workoutPlanId, gymId, dto.Exercises, cancellationToken);
    }

    public Task DeletePlanAsync(int workoutPlanId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteWorkoutPlan, new { WorkoutPlanId = workoutPlanId, GymId = gymId }, cancellationToken);

    public Task SetActiveAsync(int workoutPlanId, Guid gymId, bool isActive, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetWorkoutPlanActive, new { WorkoutPlanId = workoutPlanId, GymId = gymId, IsActive = isActive }, cancellationToken);

    public async Task<int> ClonePlanAsync(int sourcePlanId, Guid gymId, string? newPlanName, Guid? createdByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@SourceWorkoutPlanId", sourcePlanId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@NewPlanName", newPlanName);
        parameters.Add("@CreatedByUserId", createdByUserId);
        parameters.Add("@NewWorkoutPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CloneWorkoutPlan, parameters, "@NewWorkoutPlanId", cancellationToken);
    }

    public async Task<int> AssignToMemberAsync(Guid gymId, AssignWorkoutPlanDto dto, Guid? assignedByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@WorkoutPlanId", dto.WorkoutPlanId);
        parameters.Add("@AssignedByUserId", assignedByUserId);
        parameters.Add("@StartDate", dto.StartDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@EndDate", dto.EndDate?.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@DeactivatePrevious", dto.DeactivatePrevious);
        parameters.Add("@AssignedWorkoutPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.AssignWorkoutPlanToMember, parameters, "@AssignedWorkoutPlanId", cancellationToken);
    }

    public Task UnassignAsync(int assignedWorkoutPlanId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UnassignWorkoutPlan, new { AssignedWorkoutPlanId = assignedWorkoutPlanId, GymId = gymId }, cancellationToken);

    public async Task<MemberWorkoutPlanViewDto> GetMemberWorkoutAsync(int memberId, Guid? gymId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetMemberWorkoutPlan,
            new { MemberId = memberId, GymId = gymId, ActiveOnly = activeOnly },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var header = await multi.ReadSingleOrDefaultAsync<MemberWorkoutHeaderRow>();
        var exercises = (await multi.ReadAsync<WorkoutPlanExerciseDto>()).ToList();

        if (header is null)
            return new MemberWorkoutPlanViewDto { MemberId = memberId, Exercises = exercises };

        return new MemberWorkoutPlanViewDto
        {
            AssignedWorkoutPlanId = header.AssignedWorkoutPlanId,
            MemberId = header.MemberId,
            MemberName = header.MemberName,
            WorkoutPlanId = header.WorkoutPlanId,
            PlanName = header.PlanName,
            PlanDescription = header.PlanDescription,
            Goal = header.Goal,
            DurationWeeks = header.DurationWeeks,
            StartDate = header.StartDate,
            EndDate = header.EndDate,
            AssignmentNotes = header.AssignmentNotes,
            IsActive = header.IsActive,
            Exercises = exercises
        };
    }

    public async Task<int> UpsertProgressAsync(Guid gymId, UpdateWorkoutProgressDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@AssignedWorkoutPlanId", dto.AssignedWorkoutPlanId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@WorkoutPlanExerciseId", dto.WorkoutPlanExerciseId);
        parameters.Add("@IsCompleted", dto.IsCompleted);
        parameters.Add("@CompletionPercentage", dto.CompletionPercentage);
        parameters.Add("@TrainerNotes", dto.TrainerNotes);
        parameters.Add("@MemberNotes", dto.MemberNotes);
        parameters.Add("@MemberWorkoutProgressId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertMemberWorkoutProgress, parameters, "@MemberWorkoutProgressId", cancellationToken);
    }

    private Task ReplaceExercisesAsync(int planId, Guid gymId, IReadOnlyList<WorkoutPlanExerciseInputDto> exercises, CancellationToken ct)
    {
        var json = exercises.Count == 0 ? null : JsonSerializer.Serialize(exercises, JsonOptions);
        return _sp.ExecuteAsync(StoredProcedureNames.ReplaceWorkoutPlanExercises, new { WorkoutPlanId = planId, GymId = gymId, ExercisesJson = json }, ct);
    }

    private sealed class MemberWorkoutHeaderRow
    {
        public int AssignedWorkoutPlanId { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public int WorkoutPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string? PlanDescription { get; set; }
        public string? Goal { get; set; }
        public int? DurationWeeks { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? AssignmentNotes { get; set; }
        public bool IsActive { get; set; }
    }
}
