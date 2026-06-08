using System.Data;
using Dapper;
using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public MembershipRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<MembershipResponseDto> CreateAsync(
        Guid gymId,
        CreateMembershipDto dto,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@MembershipPlanId", dto.MembershipPlanId);
        parameters.Add("@StartDate", dto.StartDate, dbType: DbType.Date);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@MembershipId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var membershipId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateMembership, parameters, "@MembershipId", cancellationToken);

        return (await GetByIdAsync(membershipId, gymId, cancellationToken))!;
    }

    public Task RenewAsync(int membershipId, Guid gymId, RenewMembershipDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.RenewMembership, new
        {
            MembershipId = membershipId,
            GymId = gymId,
            dto.Notes
        }, cancellationToken);

    public Task CancelAsync(int membershipId, Guid gymId, CancelMembershipDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.CancelMembership, new
        {
            MembershipId = membershipId,
            GymId = gymId,
            dto.Notes
        }, cancellationToken);

    public async Task<MembershipResponseDto?> GetByIdAsync(int membershipId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MembershipRow>(
            StoredProcedureNames.GetMembershipById,
            new { MembershipId = membershipId, GymId = gymId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToMembershipDto(row);
    }

    public async Task<IReadOnlyList<MembershipResponseDto>> GetAllAsync(
        Guid? gymId,
        int? memberId,
        string? search,
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MembershipRow>(
            StoredProcedureNames.GetAllMemberships,
            new { GymId = gymId, MemberId = memberId, Search = search, IncludeInactive = includeInactive },
            cancellationToken);

        return rows.Select(DtoMapper.ToMembershipDto).ToList();
    }

    public async Task<IReadOnlyList<MembershipResponseDto>> GetExpiredAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MembershipRow>(
            StoredProcedureNames.GetExpiredMemberships,
            new { GymId = gymId },
            cancellationToken);

        return rows.Select(DtoMapper.ToMembershipDto).ToList();
    }
}
