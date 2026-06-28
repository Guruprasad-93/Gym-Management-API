using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.GymAdmins;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class GymAdminRepository : IGymAdminRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public GymAdminRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public Task CreateAsync(
        Guid userId,
        Guid gymId,
        string name,
        string loginIdentifier,
        string? email,
        string passwordHash,
        bool mustChangePassword,
        CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.CreateGymAdmin, new
        {
            UserId = userId,
            GymId = gymId,
            Name = name,
            LoginIdentifier = loginIdentifier.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            Password = passwordHash,
            MustChangePassword = mustChangePassword
        }, cancellationToken);

    public async Task<PagedResultDto<GymAdminDto>> GetAllAsync(
        Guid? gymId,
        PagedRequestDto paging,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", paging.Search);
        parameters.Add("@PageNumber", paging.PageNumber);
        parameters.Add("@PageSize", paging.PageSize);
        parameters.Add("@SortColumn", NormalizeSortColumn(paging.SortColumn));
        parameters.Add("@SortDirection", paging.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<GymAdminRow>(
            StoredProcedureNames.GetAllGymAdmins,
            parameters,
            cancellationToken);

        var totalCount = parameters.Get<int>("@TotalCount");

        return new PagedResultDto<GymAdminDto>
        {
            Items = rows.Select(ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = paging.PageNumber,
            PageSize = paging.PageSize
        };
    }

    public async Task<GymAdminDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<GymAdminRow>(
            StoredProcedureNames.GetGymAdminById,
            new { UserId = userId },
            cancellationToken);

        return row is null ? null : ToDto(row);
    }

    public Task UpdateAsync(Guid userId, UpdateGymAdminDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateGymAdmin, new
        {
            UserId = userId,
            dto.Name,
            Email = dto.Email.Trim().ToLowerInvariant(),
            dto.GymId
        }, cancellationToken);

    public Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetGymAdminActive, new { UserId = userId, IsActive = isActive }, cancellationToken);

    public Task ResetPasswordAsync(
        Guid userId,
        string passwordHash,
        bool mustChangePassword,
        CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ResetGymAdminPassword, new
        {
            UserId = userId,
            PasswordHash = passwordHash,
            MustChangePassword = mustChangePassword
        }, cancellationToken);

    private static GymAdminDto ToDto(GymAdminRow row) => new()
    {
        UserId = row.UserId,
        GymId = row.GymId,
        GymName = row.GymName ?? string.Empty,
        Name = row.Name,
        LoginIdentifier = row.LoginIdentifier,
        Email = row.Email ?? string.Empty,
        IsActive = row.IsActive,
        MustChangePassword = row.MustChangePassword,
        CreatedDate = row.CreatedDate
    };

    private static string NormalizeSortColumn(string sortColumn) =>
        sortColumn?.ToLowerInvariant() switch
        {
            "email" => "Email",
            "createddate" => "CreatedDate",
            _ => "Name"
        };
}
