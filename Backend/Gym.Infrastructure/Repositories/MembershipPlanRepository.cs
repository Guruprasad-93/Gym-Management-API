using System.Data;
using Dapper;
using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class MembershipPlanRepository : IMembershipPlanRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public MembershipPlanRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<MembershipPlanResponseDto> CreateAsync(
        Guid gymId,
        CreateMembershipPlanDto dto,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@PlanName", dto.PlanName);
        parameters.Add("@DurationInMonths", dto.DurationInMonths);
        parameters.Add("@Price", dto.Price);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@MembershipPlanId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var planId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateMembershipPlan, parameters, "@MembershipPlanId", cancellationToken);

        var rows = await _sp.QueryAsync<MembershipPlanRow>(
            StoredProcedureNames.GetMembershipPlans, new { GymId = gymId }, cancellationToken);

        return DtoMapper.ToMembershipPlanDto(rows.First(r => r.MembershipPlanId == planId));
    }

    public Task UpdateAsync(int planId, Guid gymId, UpdateMembershipPlanDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateMembershipPlan, new
        {
            MembershipPlanId = planId,
            GymId = gymId,
            dto.PlanName,
            dto.DurationInMonths,
            dto.Price,
            dto.Description,
            dto.IsActive
        }, cancellationToken);

    public Task DeleteAsync(int planId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteMembershipPlan, new { MembershipPlanId = planId, GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<MembershipPlanResponseDto>> GetAllAsync(
        Guid? gymId,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MembershipPlanRow>(
            StoredProcedureNames.GetMembershipPlans,
            new { GymId = gymId, IncludeInactive = includeInactive },
            cancellationToken);

        return rows.Select(DtoMapper.ToMembershipPlanDto).ToList();
    }
}
