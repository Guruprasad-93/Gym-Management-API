using System.Data;
using Dapper;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuditLogRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<long> InsertAsync(
        Guid? gymId,
        Guid? userId,
        string entityName,
        string entityId,
        string actionType,
        string? oldValueJson = null,
        string? newValueJson = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var id = await _sp.QuerySingleOrDefaultAsync<long>(
            StoredProcedureNames.AuditLogInsert,
            new
            {
                GymId = gymId,
                UserId = userId,
                EntityName = entityName,
                EntityId = entityId,
                ActionType = actionType,
                OldValueJson = oldValueJson,
                NewValueJson = newValueJson,
                IpAddress = ipAddress
            },
            cancellationToken);

        return id;
    }

    public async Task<PagedResultDto<AuditLogDto>> SearchAsync(
        Guid? gymId,
        AuditSearchQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var parameters = BuildSearchParameters(gymId, query);
        var rows = await _sp.QueryAsync<AuditLogDto>(
            StoredProcedureNames.SearchAuditLogs, parameters, cancellationToken);

        return new PagedResultDto<AuditLogDto>
        {
            Items = rows.ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<AuditLogDto>> SearchAllForExportAsync(
        Guid? gymId,
        AuditSearchQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var exportQuery = new AuditSearchQueryDto
        {
            UserId = query.UserId,
            EntityName = query.EntityName,
            ActionType = query.ActionType,
            EntityId = query.EntityId,
            Search = query.Search,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            PageNumber = 1,
            PageSize = 10000
        };
        var result = await SearchAsync(gymId, exportQuery, cancellationToken);
        return result.Items;
    }

    public async Task<AuditDashboardDto> GetDashboardAsync(
        Guid? gymId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.GetAuditLogSummary,
                new { GymId = gymId, FromDate = fromDate, ToDate = toDate },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var total = await multi.ReadSingleOrDefaultAsync<int>();
        var byEntity = (await multi.ReadAsync<AuditCountRow>()).Select(r => new AuditCountByKeyDto
        {
            Key = r.EntityName,
            Count = r.LogCount
        }).ToList();
        var byAction = (await multi.ReadAsync<AuditActionCountRow>()).Select(r => new AuditCountByKeyDto
        {
            Key = r.ActionType,
            Count = r.LogCount
        }).ToList();

        return new AuditDashboardDto
        {
            TotalLogs = total,
            ByEntity = byEntity,
            ByAction = byAction
        };
    }

    private static DynamicParameters BuildSearchParameters(Guid? gymId, AuditSearchQueryDto query)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@UserId", query.UserId);
        parameters.Add("@EntityName", query.EntityName);
        parameters.Add("@ActionType", query.ActionType);
        parameters.Add("@EntityId", query.EntityId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@FromDate", query.FromDate);
        parameters.Add("@ToDate", query.ToDate);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return parameters;
    }

    private sealed class AuditCountRow
    {
        public string EntityName { get; set; } = string.Empty;
        public int LogCount { get; set; }
    }

    private sealed class AuditActionCountRow
    {
        public string ActionType { get; set; } = string.Empty;
        public int LogCount { get; set; }
    }
}
