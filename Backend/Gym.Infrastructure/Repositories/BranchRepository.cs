using System.Data;
using Dapper;
using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public BranchRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<BranchDto> CreateAsync(Guid gymId, CreateBranchDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchName", dto.BranchName.Trim());
        parameters.Add("@BranchCode", dto.BranchCode);
        parameters.Add("@Address", dto.Address);
        parameters.Add("@City", dto.City);
        parameters.Add("@Phone", dto.Phone);
        parameters.Add("@Email", dto.Email);
        parameters.Add("@BranchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var branchId = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateBranch, parameters, "@BranchId", cancellationToken);
        if (dto.ManagerUserId.HasValue)
            await AssignManagerAsync(gymId, branchId, dto.ManagerUserId.Value, cancellationToken);
        return (await GetByIdAsync(branchId, gymId, cancellationToken))!;
    }

    public Task UpdateAsync(int branchId, Guid gymId, UpdateBranchDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateBranch, new
        {
            BranchId = branchId,
            GymId = gymId,
            BranchName = dto.BranchName.Trim(),
            dto.BranchCode,
            dto.Address,
            dto.City,
            dto.Phone,
            dto.Email
        }, cancellationToken);

    public Task SetActiveAsync(int branchId, Guid gymId, bool isActive, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.SetBranchActive, new { BranchId = branchId, GymId = gymId, IsActive = isActive }, cancellationToken);

    public Task DeleteAsync(int branchId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteBranch, new { BranchId = branchId, GymId = gymId }, cancellationToken);

    public async Task<BranchDto?> GetByIdAsync(int branchId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<BranchRow>(StoredProcedureNames.GetBranchById, new { BranchId = branchId, GymId = gymId }, cancellationToken);
        return row is null ? null : MapBranch(row);
    }

    public async Task<PagedResultDto<BranchDto>> GetPagedAsync(Guid gymId, BranchSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@Search", query.Search);
        parameters.Add("@IncludeInactive", query.IncludeInactive);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@SortColumn", query.SortColumn);
        parameters.Add("@SortDirection", query.SortDirection);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<BranchRow>(StoredProcedureNames.GetBranchesPaged, parameters, cancellationToken);
        return new PagedResultDto<BranchDto>
        {
            Items = rows.Select(MapBranch).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(Guid gymId, bool includeInactive, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BranchRow>(StoredProcedureNames.GetAllBranches, new { GymId = gymId, IncludeInactive = includeInactive }, cancellationToken);
        return rows.Select(MapBranch).ToList();
    }

    public Task AssignManagerAsync(Guid gymId, int branchId, Guid userId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", branchId);
        parameters.Add("@UserId", userId);
        parameters.Add("@BranchManagerId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.AssignBranchManager, parameters, "@BranchManagerId", cancellationToken);
    }

    public async Task<int> TransferMemberAsync(Guid gymId, TransferMemberBranchDto dto, Guid? transferredBy, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@ToBranchId", dto.ToBranchId);
        parameters.Add("@TransferredByUserId", transferredBy);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@TransferId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.TransferMemberBranch, parameters, "@TransferId", cancellationToken);
    }

    public async Task<int> TransferTrainerAsync(Guid gymId, TransferTrainerBranchDto dto, Guid? transferredBy, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@ToBranchId", dto.ToBranchId);
        parameters.Add("@TransferredByUserId", transferredBy);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@TransferId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.TransferTrainerBranch, parameters, "@TransferId", cancellationToken);
    }

    public async Task<PagedResultDto<BranchTransferDto>> GetTransferHistoryAsync(Guid gymId, BranchTransferQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@EntityType", query.EntityType);
        parameters.Add("@BranchId", query.BranchId);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<BranchTransferRow>(StoredProcedureNames.GetBranchTransferHistory, parameters, cancellationToken);
        return new PagedResultDto<BranchTransferDto>
        {
            Items = rows.Select(r => new BranchTransferDto
            {
                TransferId = r.TransferId,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                EntityName = r.EntityName,
                FromBranchId = r.FromBranchId,
                FromBranchName = r.FromBranchName,
                ToBranchId = r.ToBranchId,
                ToBranchName = r.ToBranchName,
                TransferredByName = r.TransferredByName,
                TransferDate = r.TransferDate,
                Notes = r.Notes
            }).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<BranchTargetDto> UpsertTargetAsync(Guid gymId, UpsertBranchTargetDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", dto.BranchId);
        parameters.Add("@TargetMonth", dto.TargetMonth.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@RevenueTarget", dto.RevenueTarget);
        parameters.Add("@NewMembersTarget", dto.NewMembersTarget);
        parameters.Add("@LeadConversionsTarget", dto.LeadConversionsTarget);
        parameters.Add("@TargetId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertBranchTarget, parameters, "@TargetId", cancellationToken);
        var targets = await GetTargetsAsync(gymId, dto.BranchId, dto.TargetMonth, cancellationToken);
        return targets.First();
    }

    public async Task<IReadOnlyList<BranchTargetDto>> GetTargetsAsync(Guid gymId, int? branchId, DateOnly? targetMonth, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BranchTargetRow>(StoredProcedureNames.GetBranchTargets, new
        {
            GymId = gymId,
            BranchId = branchId,
            TargetMonth = targetMonth?.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
        return rows.Select(MapTarget).ToList();
    }

    public async Task<BranchAnnouncementDto> CreateAnnouncementAsync(Guid gymId, CreateBranchAnnouncementDto dto, Guid? createdBy, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", dto.BranchId);
        parameters.Add("@Title", dto.Title.Trim());
        parameters.Add("@Message", dto.Message.Trim());
        parameters.Add("@TargetAudience", dto.TargetAudience);
        parameters.Add("@ExpiryDate", dto.ExpiryDate);
        parameters.Add("@CreatedByUserId", createdBy);
        parameters.Add("@AnnouncementId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var id = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateBranchAnnouncement, parameters, "@AnnouncementId", cancellationToken);
        var list = await GetAnnouncementsAsync(gymId, dto.BranchId, null, false, cancellationToken);
        return list.First(a => a.AnnouncementId == id);
    }

    public async Task<IReadOnlyList<BranchAnnouncementDto>> GetAnnouncementsAsync(Guid gymId, int? branchId, string? audience, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BranchAnnouncementRow>(StoredProcedureNames.GetBranchAnnouncements, new
        {
            GymId = gymId,
            BranchId = branchId,
            TargetAudience = audience,
            ActiveOnly = activeOnly
        }, cancellationToken);
        return rows.Select(r => new BranchAnnouncementDto
        {
            AnnouncementId = r.AnnouncementId,
            BranchId = r.BranchId,
            BranchName = r.BranchName,
            Title = r.Title,
            Message = r.Message,
            TargetAudience = r.TargetAudience,
            IsActive = r.IsActive,
            PublishDate = r.PublishDate,
            ExpiryDate = r.ExpiryDate
        }).ToList();
    }

    public Task DeleteAnnouncementAsync(int announcementId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteBranchAnnouncement, new { AnnouncementId = announcementId, GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<AnnouncementRecipientRow>> GetAnnouncementRecipientsAsync(Guid gymId, int announcementId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<RecipientRow>(StoredProcedureNames.GetBranchAnnouncementRecipients, new { GymId = gymId, AnnouncementId = announcementId }, cancellationToken);
        return rows.Select(r => new AnnouncementRecipientRow(r.MemberId, r.Phone, r.RecipientUserId, r.RecipientName)).ToList();
    }

    public async Task<IReadOnlyList<BranchDashboardItemDto>> GetDashboardAsync(Guid gymId, int? branchId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BranchDashboardItemDto>(StoredProcedureNames.GetBranchDashboard, new { GymId = gymId, BranchId = branchId }, cancellationToken);
        return rows.ToList();
    }

    public async Task<BranchAnalyticsDto> GetAnalyticsAsync(Guid gymId, int months, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetBranchAnalyticsComparison,
            new { GymId = gymId, Months = months },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
        var rankings = (await multi.ReadAsync<BranchAnalyticsRankingDto>()).ToList();
        var monthly = (await multi.ReadAsync<BranchMonthlyRevenueDto>()).ToList();
        return new BranchAnalyticsDto { Rankings = rankings, MonthlyRevenue = monthly };
    }

    public async Task<int> EnsureDefaultBranchAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.EnsureDefaultBranch, parameters, "@BranchId", cancellationToken);
        return parameters.Get<int>("@BranchId");
    }

    private static BranchDto MapBranch(BranchRow r) => new()
    {
        BranchId = r.BranchId,
        GymId = r.GymId,
        BranchName = r.BranchName,
        BranchCode = r.BranchCode,
        Address = r.Address,
        City = r.City,
        Phone = r.Phone,
        Email = r.Email,
        IsActive = r.IsActive,
        CreatedDate = r.CreatedDate,
        UpdatedDate = r.UpdatedDate,
        ManagerUserId = r.ManagerUserId,
        ManagerName = r.ManagerName,
        MemberCount = r.MemberCount,
        TrainerCount = r.TrainerCount
    };

    private static BranchTargetDto MapTarget(BranchTargetRow r) => new()
    {
        TargetId = r.TargetId,
        BranchId = r.BranchId,
        BranchName = r.BranchName,
        TargetMonth = DateOnly.FromDateTime(r.TargetMonth),
        RevenueTarget = r.RevenueTarget,
        NewMembersTarget = r.NewMembersTarget,
        LeadConversionsTarget = r.LeadConversionsTarget,
        ActualRevenue = r.ActualRevenue,
        ActualNewMembers = r.ActualNewMembers,
        ActualLeadConversions = r.ActualLeadConversions,
        RevenueAchievementPercent = r.RevenueTarget == 0 ? 0 : Math.Round(r.ActualRevenue / r.RevenueTarget * 100, 1),
        MembersAchievementPercent = r.NewMembersTarget == 0 ? 0 : Math.Round((decimal)r.ActualNewMembers / r.NewMembersTarget * 100, 1),
        LeadsAchievementPercent = r.LeadConversionsTarget == 0 ? 0 : Math.Round((decimal)r.ActualLeadConversions / r.LeadConversionsTarget * 100, 1)
    };

    private sealed class BranchRow
    {
        public int BranchId { get; set; }
        public Guid GymId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
        public int MemberCount { get; set; }
        public int TrainerCount { get; set; }
    }

    private sealed class BranchTransferRow
    {
        public int TransferId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? EntityName { get; set; }
        public int? FromBranchId { get; set; }
        public string? FromBranchName { get; set; }
        public int ToBranchId { get; set; }
        public string? ToBranchName { get; set; }
        public string? TransferredByName { get; set; }
        public DateTime TransferDate { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class BranchTargetRow
    {
        public int TargetId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime TargetMonth { get; set; }
        public decimal RevenueTarget { get; set; }
        public int NewMembersTarget { get; set; }
        public int LeadConversionsTarget { get; set; }
        public decimal ActualRevenue { get; set; }
        public int ActualNewMembers { get; set; }
        public int ActualLeadConversions { get; set; }
    }

    private sealed class BranchAnnouncementRow
    {
        public int AnnouncementId { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    private sealed class RecipientRow
    {
        public int MemberId { get; set; }
        public string? Phone { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientName { get; set; }
    }
}
