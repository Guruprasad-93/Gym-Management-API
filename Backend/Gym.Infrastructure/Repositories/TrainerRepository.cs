using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Trainers;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public TrainerRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<TrainerDto> CreateAsync(Guid gymId, CreateTrainerDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", dto.UserId);
        parameters.Add("@Specialization", dto.Specialization);
        parameters.Add("@Bio", dto.Bio);
        parameters.Add("@TrainerId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var trainerId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateTrainer, parameters, "@TrainerId", cancellationToken);

        return (await GetByIdAsync(trainerId, gymId, cancellationToken))!;
    }

    public async Task<TrainerDto?> GetByIdAsync(int trainerId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<TrainerRow>(
            StoredProcedureNames.GetTrainerById,
            new { TrainerId = trainerId, GymId = gymId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToTrainerDto(row);
    }

    public async Task<TrainerDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<TrainerRow>(
            StoredProcedureNames.GetTrainerByUserId,
            new { UserId = userId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToTrainerDto(row);
    }

    public async Task<PagedResultDto<TrainerDto>> GetPagedAsync(
        Guid? gymId,
        string? search,
        bool includeInactive,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", search);
        parameters.Add("@IncludeInactive", includeInactive);
        parameters.Add("@PageNumber", paging.PageNumber);
        parameters.Add("@PageSize", paging.PageSize);
        parameters.Add("@SortColumn", NormalizeSortColumn(paging.SortColumn));
        parameters.Add("@SortDirection", paging.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<TrainerRow>(
            StoredProcedureNames.GetAllTrainers,
            parameters,
            cancellationToken);

        return new PagedResultDto<TrainerDto>
        {
            Items = rows.Select(DtoMapper.ToTrainerDto).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize
        };
    }

    public Task UpdateAsync(int trainerId, Guid gymId, UpdateTrainerDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateTrainer, new
        {
            TrainerId = trainerId,
            GymId = gymId,
            dto.Specialization,
            dto.Bio,
            dto.IsActive
        }, cancellationToken);

    public Task DeleteAsync(int trainerId, Guid gymId, bool unassignMembers = true, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteTrainer, new
        {
            TrainerId = trainerId,
            GymId = gymId,
            UnassignMembers = unassignMembers
        }, cancellationToken);

    public Task AssignMemberAsync(int trainerId, int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.AssignMemberToTrainer, new
        {
            TrainerId = trainerId,
            MemberId = memberId,
            GymId = gymId
        }, cancellationToken);

    public Task RemoveMemberAssignmentAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.RemoveTrainerAssignment, new
        {
            MemberId = memberId,
            GymId = gymId
        }, cancellationToken);

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(
        int trainerId,
        Guid? gymId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberRow>(
            StoredProcedureNames.GetTrainerMembers,
            new { TrainerId = trainerId, GymId = gymId, Search = search },
            cancellationToken);

        return rows.Select(DtoMapper.ToMemberDto).ToList();
    }

    public async Task<IReadOnlyList<MemberDto>> GetUnassignedMembersAsync(
        Guid gymId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberRow>(
            StoredProcedureNames.GetUnassignedMembers,
            new { GymId = gymId, Search = search },
            cancellationToken);

        return rows.Select(DtoMapper.ToMemberDto).ToList();
    }

    public async Task<TrainerDashboardDto?> GetDashboardAsync(
        int trainerId,
        Guid? gymId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<TrainerDashboardRow>(
            StoredProcedureNames.GetTrainerDashboard,
            new { TrainerId = trainerId, GymId = gymId },
            cancellationToken);

        return row is null
            ? null
            : new TrainerDashboardDto
            {
                TrainerId = row.TrainerId,
                AssignedActiveMembers = row.AssignedActiveMembers,
                AssignedInactiveMembers = row.AssignedInactiveMembers,
                UnassignedMembersInGym = row.UnassignedMembersInGym,
                ActiveDietPlans = row.ActiveDietPlans,
                ActiveWorkoutPlans = row.ActiveWorkoutPlans
            };
    }

    private static string NormalizeSortColumn(string? sortColumn) => sortColumn?.ToLowerInvariant() switch
    {
        "fullname" or "username" or "name" or "user" => "UserName",
        "email" or "useremail" => "UserEmail",
        "specialization" or "speciality" => "Specialization",
        "createdat" or "createddate" => "CreatedAt",
        "assignedmembercount" or "members" => "UserName",
        _ => "UserName"
    };
}
