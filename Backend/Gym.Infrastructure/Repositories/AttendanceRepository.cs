using System.Data;
using Dapper;
using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public AttendanceRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AttendanceStatusDto>> GetStatusesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<AttendanceStatusDto>(
            StoredProcedureNames.GetAttendanceStatuses, null, cancellationToken);
        return rows;
    }

    public async Task<MemberAttendanceDto> CheckInMemberAsync(
        Guid gymId, int memberId, int? trainerId, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@Notes", notes);
        parameters.Add("@MemberAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.MemberAttendanceCheckIn, parameters, "@MemberAttendanceId", cancellationToken);

        return (await GetMemberAttendanceByIdAsync(id, gymId, null, cancellationToken))!;
    }

    public async Task<MemberAttendanceDto> CheckOutMemberAsync(
        Guid gymId, int? memberId, int? memberAttendanceId, string checkoutType, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@TargetAttendanceId", memberAttendanceId);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@Notes", notes);
        parameters.Add("@CheckoutType", checkoutType);
        parameters.Add("@MemberAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.MemberAttendanceCheckOut, parameters, "@MemberAttendanceId", cancellationToken);

        return (await GetMemberAttendanceByIdAsync(id, gymId, null, cancellationToken))!;
    }

    public async Task<AttendanceSettingsDto> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<AttendanceSettingsRow>(
            StoredProcedureNames.GetAttendanceSettings,
            new { GymId = gymId },
            cancellationToken)
            ?? throw new KeyNotFoundException("Attendance settings not found.");

        return MapSettings(row);
    }

    public Task UpdateSettingsAsync(Guid gymId, UpdateAttendanceSettingsDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(
            StoredProcedureNames.UpsertAttendanceSettings,
            new
            {
                GymId = gymId,
                OpeningTime = dto.OpeningTime.ToTimeSpan(),
                ClosingTime = dto.ClosingTime.ToTimeSpan(),
                dto.AutoCheckoutEnabled,
                dto.UseClosingTimeForAutoCheckout,
                dto.CheckoutReminderMinutesBefore,
                dto.TimeZoneId,
                dto.Is24Hours,
                dto.MaximumSessionHours
            },
            cancellationToken);

    public async Task<int> ProcessAutoCheckoutAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@ProcessedCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.ProcessAttendanceAutoCheckout, parameters, cancellationToken);
        return parameters.Get<int>("@ProcessedCount");
    }

    public async Task<MemberTodayVisitDto?> GetMemberTodayVisitAsync(
        Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberTodayVisitRow>(
            StoredProcedureNames.GetMemberTodayVisit,
            new { GymId = gymId, MemberId = memberId },
            cancellationToken);

        return row is null ? null : MapTodayVisit(row);
    }

    public async Task<MemberAttendanceDto> MarkMemberAsync(
        Guid gymId, MarkAttendanceDto dto, Guid? markedByUserId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@AttendanceDate", dto.AttendanceDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@AttendanceStatusId", dto.AttendanceStatusId);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@MemberAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.MemberAttendanceMark, parameters, "@MemberAttendanceId", cancellationToken);

        return (await GetMemberAttendanceByIdAsync(id, gymId, null, cancellationToken))!;
    }

    public async Task<IReadOnlyList<MemberAttendanceDto>> GetTodayAsync(
        Guid? gymId, int? trainerId, string? search,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberAttendanceRow>(
            StoredProcedureNames.GetTodayMemberAttendance,
            new { GymId = gymId, TrainerId = trainerId, Search = search },
            cancellationToken);
        return rows.Select(MapMember).ToList();
    }

    public async Task<PagedResultDto<MemberAttendanceDto>> GetByDateRangeAsync(
        Guid? gymId, int? trainerId, AttendanceQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var from = query.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@MemberId", query.MemberId);
        parameters.Add("@FromDate", from.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@ToDate", to.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@StatusId", query.StatusId);
        parameters.Add("@OpenOnly", query.OpenOnly ?? false);
        parameters.Add("@CheckoutTypeFilter", query.CheckoutTypeFilter);
        parameters.Add("@Search", query.Search);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@SortColumn", query.SortColumn);
        parameters.Add("@SortDirection", query.SortDirection);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<MemberAttendanceRow>(
            StoredProcedureNames.GetMemberAttendanceByDateRange, parameters, cancellationToken);

        return new PagedResultDto<MemberAttendanceDto>
        {
            Items = rows.Select(MapMember).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public Task<PagedResultDto<MemberAttendanceDto>> GetMemberHistoryAsync(
        Guid? gymId, int? trainerId, int memberId, AttendanceQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query.MemberId = memberId;
        return GetByDateRangeAsync(gymId, trainerId, query, cancellationToken);
    }

    public async Task<AttendanceDashboardDto> GetDashboardAsync(
        Guid? gymId, int? trainerId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<AttendanceDashboardRow>(
            StoredProcedureNames.GetAttendanceDashboard,
            new { GymId = gymId, TrainerId = trainerId },
            cancellationToken);

        return row is null
            ? new AttendanceDashboardDto()
            : new AttendanceDashboardDto
            {
                TotalActiveMembers = row.TotalActiveMembers,
                MembersPresentToday = row.MembersPresentToday,
                CurrentlyCheckedIn = row.CurrentlyCheckedIn,
                AbsentToday = row.AbsentToday,
                CheckedOutToday = row.CheckedOutToday,
                AutoCheckedOutToday = row.AutoCheckedOutToday,
                ManualCheckOutToday = row.ManualCheckOutToday
            };
    }

    public async Task<PagedResultDto<ForgotCheckOutReportItemDto>> GetForgotCheckOutReportAsync(
        Guid? gymId, ForgotCheckOutReportQueryDto query, CancellationToken cancellationToken = default)
    {
        var from = query.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var to = query.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", query.BranchId);
        parameters.Add("@MemberId", query.MemberId);
        parameters.Add("@FromDate", from.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@ToDate", to.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<ForgotCheckOutReportRow>(
            StoredProcedureNames.GetForgotCheckOutReport, parameters, cancellationToken);

        return new PagedResultDto<ForgotCheckOutReportItemDto>
        {
            Items = rows.Select(r => new ForgotCheckOutReportItemDto
            {
                MemberId = r.MemberId,
                MemberName = r.MemberName,
                BranchId = r.BranchId,
                BranchName = r.BranchName,
                TotalAutoCheckOutCount = r.TotalAutoCheckOutCount,
                LastAutoCheckOutAt = r.LastAutoCheckOutAt,
                LastAutoCheckOutDate = r.LastAutoCheckOutDate.HasValue
                    ? DateOnly.FromDateTime(r.LastAutoCheckOutDate.Value)
                    : null
            }).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<DailyAttendanceReportDto> GetDailyReportAsync(
        Guid? gymId, int? trainerId, DateOnly reportDate, bool openOnly, string? checkoutTypeFilter,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                StoredProcedureNames.GetDailyAttendanceReport,
                new
                {
                    GymId = gymId,
                    TrainerId = trainerId,
                    ReportDate = reportDate.ToDateTime(TimeOnly.MinValue),
                    OpenOnly = openOnly,
                    CheckoutTypeFilter = checkoutTypeFilter
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var counts = (await multi.ReadAsync<DailyAttendanceStatusCountDto>()).ToList();
        var details = (await multi.ReadAsync<DailyAttendanceDetailDto>()).ToList();

        foreach (var c in counts)
            c.ReportDate = reportDate;

        return new DailyAttendanceReportDto
        {
            ReportDate = reportDate,
            StatusCounts = counts,
            Details = details
        };
    }

    public async Task<MonthlyAttendanceReportDto> GetMonthlyReportAsync(
        Guid? gymId, int? trainerId, int year, int month,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MonthlyMemberAttendanceDto>(
            StoredProcedureNames.GetMonthlyAttendanceReport,
            new { GymId = gymId, TrainerId = trainerId, Year = year, Month = month },
            cancellationToken);

        return new MonthlyAttendanceReportDto
        {
            Year = year,
            Month = month,
            Members = rows.ToList()
        };
    }

    public async Task<TrainerAttendanceDto> CheckInTrainerAsync(
        Guid gymId, int trainerId, Guid? markedByUserId, string? notes,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@Notes", notes);
        parameters.Add("@TrainerAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.TrainerAttendanceCheckIn, parameters, "@TrainerAttendanceId", cancellationToken);

        return (await GetTrainerByIdAsync(id, gymId, cancellationToken))!;
    }

    public async Task<TrainerAttendanceDto> CheckOutTrainerAsync(
        Guid gymId, int trainerId, Guid? markedByUserId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@TrainerAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var id = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.TrainerAttendanceCheckOut, parameters, "@TrainerAttendanceId", cancellationToken);

        return (await GetTrainerByIdAsync(id, gymId, cancellationToken))!;
    }

    public async Task<PagedResultDto<TrainerAttendanceDto>> GetTrainerAttendanceAsync(
        Guid? gymId, int? trainerId, DateOnly from, DateOnly to, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", trainerId);
        parameters.Add("@FromDate", from.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@ToDate", to.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@PageNumber", pageNumber);
        parameters.Add("@PageSize", pageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<TrainerAttendanceRow>(
            StoredProcedureNames.GetTrainerAttendanceByDateRange, parameters, cancellationToken);

        return new PagedResultDto<TrainerAttendanceDto>
        {
            Items = rows.Select(MapTrainer).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private async Task<MemberAttendanceDto?> GetMemberAttendanceByIdAsync(
        int id, Guid? gymId, int? trainerId, CancellationToken cancellationToken)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberAttendanceRow>(
            StoredProcedureNames.GetMemberAttendanceById,
            new { MemberAttendanceId = id, GymId = gymId, TrainerId = trainerId },
            cancellationToken);
        return row is null ? null : MapMember(row);
    }

    private async Task<TrainerAttendanceDto?> GetTrainerByIdAsync(
        int id, Guid gymId, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", (int?)null);
        parameters.Add("@FromDate", DateTime.UtcNow.Date.AddYears(-1));
        parameters.Add("@ToDate", DateTime.UtcNow.Date.AddYears(1));
        parameters.Add("@PageNumber", 1);
        parameters.Add("@PageSize", 1000);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<TrainerAttendanceRow>(
            StoredProcedureNames.GetTrainerAttendanceByDateRange, parameters, cancellationToken);

        var row = rows.FirstOrDefault(r => r.TrainerAttendanceId == id);
        return row is null ? null : MapTrainer(row);
    }

    private static MemberAttendanceDto MapMember(MemberAttendanceRow r) => new()
    {
        MemberAttendanceId = r.MemberAttendanceId,
        GymId = r.GymId,
        MemberId = r.MemberId,
        MemberName = r.MemberName,
        MemberEmail = r.MemberEmail,
        TrainerId = r.TrainerId,
        TrainerName = r.TrainerName,
        AttendanceStatusId = r.AttendanceStatusId,
        StatusCode = r.StatusCode,
        StatusName = r.StatusName,
        AttendanceDate = DateOnly.FromDateTime(r.AttendanceDate),
        CheckInAt = r.CheckInAt,
        CheckOutAt = r.CheckOutAt,
        CheckoutType = r.CheckoutType,
        IsAutoCheckout = r.IsAutoCheckout,
        IsCurrentlyCheckedIn = r.CheckOutAt is null && r.StatusCode == "CHECKED_IN",
        MarkedByName = r.MarkedByName,
        Notes = r.Notes,
        CreatedAt = r.CreatedAt
    };

    private static AttendanceSettingsDto MapSettings(AttendanceSettingsRow r) => new()
    {
        GymId = r.GymId,
        OpeningTime = TimeOnly.FromTimeSpan(r.OpeningTime),
        ClosingTime = TimeOnly.FromTimeSpan(r.ClosingTime),
        AutoCheckoutEnabled = r.AutoCheckoutEnabled,
        UseClosingTimeForAutoCheckout = r.UseClosingTimeForAutoCheckout,
        CheckoutReminderMinutesBefore = r.CheckoutReminderMinutesBefore,
        TimeZoneId = r.TimeZoneId,
        Is24Hours = r.Is24Hours,
        MaximumSessionHours = r.MaximumSessionHours
    };

    private static MemberTodayVisitDto MapTodayVisit(MemberTodayVisitRow r) => new()
    {
        CheckInAt = r.CheckInAt,
        CheckOutAt = r.CheckOutAt,
        StatusCode = r.StatusCode,
        StatusName = r.StatusName,
        CheckoutType = r.CheckoutType,
        IsAutoCheckout = r.IsAutoCheckout,
        IsCurrentlyCheckedIn = r.IsCurrentlyCheckedIn,
        CheckedOutByName = r.CheckedOutByName
    };

    private static TrainerAttendanceDto MapTrainer(TrainerAttendanceRow r) => new()
    {
        TrainerAttendanceId = r.TrainerAttendanceId,
        GymId = r.GymId,
        TrainerId = r.TrainerId,
        TrainerName = r.TrainerName,
        AttendanceStatusId = r.AttendanceStatusId,
        StatusName = r.StatusName,
        AttendanceDate = DateOnly.FromDateTime(r.AttendanceDate),
        CheckInAt = r.CheckInAt,
        CheckOutAt = r.CheckOutAt,
        Notes = r.Notes,
        CreatedAt = r.CreatedAt
    };
}
