using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Members;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public MemberRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<MemberResponseDto> CreateAsync(
        Guid gymId,
        Guid userId,
        CreateMemberDto dto,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", userId);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@DateOfBirth", dto.DateOfBirth, dbType: DbType.Date);
        parameters.Add("@Gender", dto.Gender);
        parameters.Add("@Height", dto.Height);
        parameters.Add("@Weight", dto.Weight);
        parameters.Add("@Phone", dto.Phone);
        parameters.Add("@Address", dto.Address);
        parameters.Add("@EmergencyContact", dto.EmergencyContact);
        parameters.Add("@JoinDate", dto.JoinDate, dbType: DbType.Date);
        parameters.Add("@MemberId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var memberId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateMember, parameters, "@MemberId", cancellationToken);

        return (await GetByIdAsync(memberId, gymId, null, cancellationToken))!;
    }

    public async Task<MemberResponseDto?> GetByIdAsync(
        int memberId,
        Guid? gymId,
        int? trainerId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberRow>(
            StoredProcedureNames.GetMemberById,
            new { MemberId = memberId, GymId = gymId, TrainerId = trainerId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToMemberDto(row);
    }

    public async Task<MemberResponseDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberRow>(
            StoredProcedureNames.GetMemberByUserId,
            new { UserId = userId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToMemberDto(row);
    }

    public async Task<PagedResultDto<MemberResponseDto>> GetPagedAsync(
        Guid? gymId,
        int? trainerId,
        string? search,
        bool includeInactive,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@Search", search);
        parameters.Add("@IncludeInactive", includeInactive);
        parameters.Add("@PageNumber", paging.PageNumber);
        parameters.Add("@PageSize", paging.PageSize);
        parameters.Add("@SortColumn", NormalizeSortColumn(paging.SortColumn));
        parameters.Add("@SortDirection", paging.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<MemberRow>(
            StoredProcedureNames.GetAllMembers,
            parameters,
            cancellationToken);

        return new PagedResultDto<MemberResponseDto>
        {
            Items = rows.Select(DtoMapper.ToMemberDto).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize
        };
    }

    public Task UpdateAsync(int memberId, Guid gymId, UpdateMemberDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateMember, new
        {
            MemberId = memberId,
            GymId = gymId,
            dto.FullName,
            LoginIdentifier = string.IsNullOrWhiteSpace(dto.LoginIdentifier)
                ? null
                : dto.LoginIdentifier.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
            dto.TrainerId,
            dto.DateOfBirth,
            dto.Gender,
            dto.Height,
            dto.Weight,
            dto.Phone,
            dto.Address,
            dto.EmergencyContact,
            dto.IsActive
        }, cancellationToken);

    public Task DeleteAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteMember, new { MemberId = memberId, GymId = gymId }, cancellationToken);

    public Task ActivateAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ActivateMember, new { MemberId = memberId, GymId = gymId }, cancellationToken);

    public Task DeactivateAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeactivateMember, new { MemberId = memberId, GymId = gymId }, cancellationToken);

    public Task AssignTrainerAsync(int memberId, int trainerId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.AssignTrainerToMember, new
        {
            MemberId = memberId,
            TrainerId = trainerId,
            GymId = gymId
        }, cancellationToken);

    public Task RemoveTrainerAssignmentAsync(int memberId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.RemoveTrainerAssignment, new { MemberId = memberId, GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<MemberPaymentHistoryDto>> GetPaymentHistoryAsync(
        int memberId,
        Guid? gymId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PaymentRow>(
            StoredProcedureNames.GetMemberPaymentHistory,
            new { MemberId = memberId, GymId = gymId },
            cancellationToken);

        return rows.Select(r => new MemberPaymentHistoryDto
        {
            Id = r.PaymentId,
            MembershipId = r.MembershipId,
            Amount = r.Amount,
            PaymentDate = DateOnly.FromDateTime(r.PaymentDate),
            PaymentMethod = r.PaymentMethod,
            TransactionReference = r.TransactionReference,
            Status = r.Status,
            Notes = r.Notes
        }).ToList();
    }

    public async Task<IReadOnlyList<MemberProgressDto>> GetProgressAsync(
        int memberId,
        Guid? gymId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberProgressRow>(
            StoredProcedureNames.GetMemberProgress,
            new { MemberId = memberId, GymId = gymId },
            cancellationToken);

        return rows.Select(r => new MemberProgressDto
        {
            ProgressType = r.ProgressType,
            RecordedDate = r.RecordedDate,
            Detail = r.Detail,
            WeightKg = r.WeightKg,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public Task<Guid?> GetGymIdAsync(int memberId, CancellationToken cancellationToken = default) =>
        _sp.QuerySingleOrDefaultAsync<Guid?>(StoredProcedureNames.MemberGetGymId, new { MemberId = memberId }, cancellationToken);

    private static string NormalizeSortColumn(string? sortColumn) => sortColumn?.ToLowerInvariant() switch
    {
        "fullname" or "name" or "username" => "FullName",
        "phone" => "Phone",
        "joindate" => "JoinDate",
        _ => "FullName"
    };
}
