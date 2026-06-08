using System.Data;
using Dapper;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public BookingRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<BookingSettingsDto> GetSettingsAsync(Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<BookingSettingsRow>(StoredProcedureNames.GetBookingSettings, new { GymId = gymId }, cancellationToken);
        return row is null ? new BookingSettingsDto { GymId = gymId } : MapSettings(row);
    }

    public Task UpdateSettingsAsync(Guid gymId, UpdateBookingSettingsDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpsertBookingSettings, new
        {
            GymId = gymId,
            dto.MaxBookingsPerDay,
            dto.AllowWaitlist,
            dto.CancellationWindowHours,
            dto.ReminderMinutesBefore
        }, cancellationToken);

    public async Task<int> CreateScheduleAsync(Guid gymId, CreateClassScheduleDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", dto.BranchId);
        parameters.Add("@ClassName", dto.ClassName);
        parameters.Add("@Description", dto.Description);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@DayOfWeek", dto.DayOfWeek);
        parameters.Add("@StartTime", dto.StartTime);
        parameters.Add("@EndTime", dto.EndTime);
        parameters.Add("@Capacity", dto.Capacity);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateClassSchedule, parameters, "@Id", cancellationToken);
    }

    public Task UpdateScheduleAsync(Guid gymId, UpdateClassScheduleDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateClassSchedule, new
        {
            GymId = gymId,
            Id = dto.Id,
            dto.BranchId,
            dto.ClassName,
            dto.Description,
            dto.TrainerId,
            dto.DayOfWeek,
            dto.StartTime,
            dto.EndTime,
            dto.Capacity,
            dto.Status
        }, cancellationToken);

    public async Task<ClassScheduleDto?> GetScheduleByIdAsync(Guid gymId, int id, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<ClassScheduleRow>(StoredProcedureNames.GetClassScheduleById, new { GymId = gymId, Id = id }, cancellationToken);
        return row is null ? null : MapSchedule(row);
    }

    public async Task<PagedResultDto<ClassScheduleDto>> GetSchedulesPagedAsync(Guid gymId, ClassScheduleQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", query.BranchId);
        parameters.Add("@Status", query.Status);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<ClassScheduleRow>(StoredProcedureNames.GetClassSchedulesPaged, parameters, cancellationToken);
        return new PagedResultDto<ClassScheduleDto>
        {
            Items = rows.Select(MapSchedule).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public Task DeleteScheduleAsync(Guid gymId, int id, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.DeleteClassSchedule, new { GymId = gymId, Id = id }, cancellationToken);

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(Guid gymId, AvailableSlotsQueryDto query, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<AvailableSlotRow>(StoredProcedureNames.GetAvailableSlots, new
        {
            GymId = gymId,
            query.BranchId,
            FromDate = query.FromDate.Date,
            ToDate = query.ToDate.Date
        }, cancellationToken);
        return rows.Select(MapSlot).ToList();
    }

    public async Task<(int BookingId, string? Error)> CreateBookingAsync(Guid gymId, int memberId, BookSlotDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@ClassScheduleId", dto.ClassScheduleId);
        parameters.Add("@BookingDate", dto.BookingDate.Date);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CreateSlotBooking, parameters, cancellationToken);
        return (parameters.Get<int>("@Id"), parameters.Get<string?>("@ErrorMessage"));
    }

    public async Task<PromoteWaitlistResultDto> CancelBookingAsync(Guid gymId, int bookingId, int? memberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BookingId", bookingId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@PromotedBookingId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@PromotedMemberId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CancelSlotBooking, parameters, cancellationToken);
        return new PromoteWaitlistResultDto
        {
            PromotedBookingId = parameters.Get<int>("@PromotedBookingId"),
            PromotedMemberId = parameters.Get<int>("@PromotedMemberId")
        };
    }

    public async Task<(int WaitlistId, string? Error)> JoinWaitlistAsync(Guid gymId, int memberId, JoinWaitlistDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@ClassScheduleId", dto.ClassScheduleId);
        parameters.Add("@BookingDate", dto.BookingDate.Date);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.JoinBookingWaitlist, parameters, cancellationToken);
        return (parameters.Get<int>("@Id"), parameters.Get<string?>("@ErrorMessage"));
    }

    public async Task<PagedResultDto<SlotBookingDto>> GetBookingsPagedAsync(Guid gymId, BookingQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", query.BranchId);
        parameters.Add("@MemberId", query.MemberId);
        parameters.Add("@ClassScheduleId", query.ClassScheduleId);
        parameters.Add("@Status", query.Status);
        parameters.Add("@FromDate", query.FromDate?.Date);
        parameters.Add("@ToDate", query.ToDate?.Date);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var rows = await _sp.QueryAsync<SlotBookingRow>(StoredProcedureNames.GetSlotBookingsPaged, parameters, cancellationToken);
        return new PagedResultDto<SlotBookingDto>
        {
            Items = rows.Select(MapBooking).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<(int BookingId, string? Error)> QrCheckInAsync(Guid gymId, int memberId, string qrToken, Guid markedByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@QrToken", qrToken);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@BookingId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.BookingQrCheckIn, parameters, cancellationToken);
        return (parameters.Get<int>("@BookingId"), parameters.Get<string?>("@ErrorMessage"));
    }

    public Task ProcessNoShowsAsync(Guid? gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ProcessNoShowBookings, new { GymId = gymId }, cancellationToken);

    public async Task<IReadOnlyList<BookingReminderRowDto>> GetBookingsForReminderAsync(int minutesBefore, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<BookingReminderRow>(StoredProcedureNames.GetBookingsForReminder, new { MinutesBefore = minutesBefore }, cancellationToken);
        return rows.Select(r => new BookingReminderRowDto
        {
            BookingId = r.BookingId,
            GymId = r.GymId,
            MemberId = r.MemberId,
            UserId = r.UserId,
            Phone = r.Phone,
            MemberName = r.MemberName,
            ClassName = r.ClassName,
            BookingDate = r.BookingDate,
            StartTime = r.StartTime,
            TrainerId = r.TrainerId,
            TrainerName = r.TrainerName,
            TrainerUserId = r.TrainerUserId
        }).ToList();
    }

    public async Task<IReadOnlyList<TrainerScheduleDto>> GetTrainerScheduleAsync(Guid gymId, int trainerId, TrainerScheduleQueryDto query, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrainerScheduleRow>(StoredProcedureNames.GetTrainerSchedule, new
        {
            GymId = gymId,
            TrainerId = trainerId,
            FromDate = query.FromDate.Date,
            ToDate = query.ToDate.Date
        }, cancellationToken);
        return rows.Select(r => new TrainerScheduleDto
        {
            ClassScheduleId = r.ClassScheduleId,
            ClassName = r.ClassName,
            BranchId = r.BranchId,
            BranchName = r.BranchName,
            DayOfWeek = r.DayOfWeek,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Capacity = r.Capacity,
            Status = r.Status,
            BookingCount = r.BookingCount
        }).ToList();
    }

    public async Task<int> CreateTrainerAvailabilityAsync(Guid gymId, CreateTrainerAvailabilityDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@BranchId", dto.BranchId);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@DayOfWeek", dto.DayOfWeek);
        parameters.Add("@StartTime", dto.StartTime);
        parameters.Add("@EndTime", dto.EndTime);
        parameters.Add("@IsAvailable", true);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateTrainerAvailability, parameters, "@Id", cancellationToken);
    }

    public async Task<IReadOnlyList<TrainerAvailabilityDto>> GetTrainerAvailabilityAsync(Guid gymId, int? trainerId, int? branchId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrainerAvailabilityRow>(StoredProcedureNames.GetTrainerAvailability, new { GymId = gymId, TrainerId = trainerId, BranchId = branchId }, cancellationToken);
        return rows.Select(r => new TrainerAvailabilityDto
        {
            Id = r.Id,
            GymId = r.GymId,
            BranchId = r.BranchId,
            BranchName = r.BranchName,
            TrainerId = r.TrainerId,
            TrainerName = r.TrainerName,
            DayOfWeek = r.DayOfWeek,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            IsAvailable = r.IsAvailable
        }).ToList();
    }

    public async Task<BookingAnalyticsDto> GetAnalyticsAsync(Guid gymId, int? branchId, int days, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetBookingAnalytics,
            new { GymId = gymId, BranchId = branchId, Days = days },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var summary = await multi.ReadSingleOrDefaultAsync<AnalyticsSummaryRow>();
        var trend = (await multi.ReadAsync<ChartRow>()).ToList();
        var popular = (await multi.ReadAsync<ChartRow>()).ToList();
        var peak = (await multi.ReadAsync<ChartRow>()).ToList();
        var branches = (await multi.ReadAsync<ChartRow>()).ToList();

        return new BookingAnalyticsDto
        {
            TotalBookings = summary?.TotalBookings ?? 0,
            TodaysBookings = summary?.TodaysBookings ?? 0,
            OccupancyPercent = summary?.OccupancyPercent ?? 0,
            NoShowPercent = summary?.NoShowPercent ?? 0,
            CancellationPercent = summary?.CancellationPercent ?? 0,
            BookingTrend = trend.Select(MapChart).ToList(),
            PopularClasses = popular.Select(MapChart).ToList(),
            PeakHours = peak.Select(r => new BookingChartPointDto { Label = $"{r.Label}:00", BookingCount = r.BookingCount }).ToList(),
            BranchComparison = branches.Select(MapChart).ToList()
        };
    }

    public async Task<IReadOnlyList<Guid>> GetGymsForJobAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<GymRow>(StoredProcedureNames.GetGymsForBookingJob, cancellationToken: cancellationToken);
        return rows.Select(r => r.GymId).ToList();
    }

    private static BookingSettingsDto MapSettings(BookingSettingsRow row) => new()
    {
        GymId = row.GymId,
        MaxBookingsPerDay = row.MaxBookingsPerDay,
        AllowWaitlist = row.AllowWaitlist,
        CancellationWindowHours = row.CancellationWindowHours,
        ReminderMinutesBefore = row.ReminderMinutesBefore
    };

    private static ClassScheduleDto MapSchedule(ClassScheduleRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        BranchId = row.BranchId,
        BranchName = row.BranchName,
        ClassName = row.ClassName,
        Description = row.Description,
        TrainerId = row.TrainerId,
        TrainerName = row.TrainerName,
        DayOfWeek = row.DayOfWeek,
        StartTime = row.StartTime,
        EndTime = row.EndTime,
        Capacity = row.Capacity,
        Status = row.Status,
        CreatedDate = row.CreatedDate
    };

    private static AvailableSlotDto MapSlot(AvailableSlotRow row) => new()
    {
        ClassScheduleId = row.ClassScheduleId,
        GymId = row.GymId,
        BranchId = row.BranchId,
        BranchName = row.BranchName,
        ClassName = row.ClassName,
        Description = row.Description,
        TrainerId = row.TrainerId,
        TrainerName = row.TrainerName,
        BookingDate = row.BookingDate,
        StartTime = row.StartTime,
        EndTime = row.EndTime,
        Capacity = row.Capacity,
        RemainingCapacity = row.RemainingCapacity,
        WaitlistCount = row.WaitlistCount
    };

    private static SlotBookingDto MapBooking(SlotBookingRow row) => new()
    {
        Id = row.Id,
        GymId = row.GymId,
        BranchId = row.BranchId,
        BranchName = row.BranchName,
        MemberId = row.MemberId,
        MemberName = row.MemberName,
        ClassScheduleId = row.ClassScheduleId,
        ClassName = row.ClassName,
        StartTime = row.StartTime,
        EndTime = row.EndTime,
        BookingDate = row.BookingDate,
        Status = row.Status,
        CheckInTime = row.CheckInTime,
        CreatedDate = row.CreatedDate,
        TrainerName = row.TrainerName
    };

    private static BookingChartPointDto MapChart(ChartRow row) => new() { Label = row.Label?.ToString() ?? string.Empty, BookingCount = row.BookingCount };

    private sealed class BookingSettingsRow
    {
        public Guid GymId { get; set; }
        public int MaxBookingsPerDay { get; set; }
        public bool AllowWaitlist { get; set; }
        public int CancellationWindowHours { get; set; }
        public int ReminderMinutesBefore { get; set; }
    }

    private sealed class ClassScheduleRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    private sealed class AvailableSlotRow
    {
        public int ClassScheduleId { get; set; }
        public Guid GymId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int RemainingCapacity { get; set; }
        public int WaitlistCount { get; set; }
    }

    private sealed class SlotBookingRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int ClassScheduleId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TrainerName { get; set; } = string.Empty;
    }

    private sealed class BookingReminderRow
    {
        public int BookingId { get; set; }
        public Guid GymId { get; set; }
        public int MemberId { get; set; }
        public Guid UserId { get; set; }
        public string? Phone { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public Guid TrainerUserId { get; set; }
    }

    private sealed class TrainerScheduleRow
    {
        public int ClassScheduleId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }

    private sealed class TrainerAvailabilityRow
    {
        public int Id { get; set; }
        public Guid GymId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    private sealed class AnalyticsSummaryRow
    {
        public int TotalBookings { get; set; }
        public int TodaysBookings { get; set; }
        public decimal OccupancyPercent { get; set; }
        public decimal NoShowPercent { get; set; }
        public decimal CancellationPercent { get; set; }
    }

    private sealed class ChartRow
    {
        public object? Label { get; set; }
        public int BookingCount { get; set; }
    }

    private sealed class GymRow
    {
        public Guid GymId { get; set; }
    }
}
