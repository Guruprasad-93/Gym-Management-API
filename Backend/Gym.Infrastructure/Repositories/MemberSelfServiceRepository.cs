using System.Data;
using Dapper;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class MemberSelfServiceRepository : IMemberSelfServiceRepository
{
    private readonly IStoredProcedureExecutor _sp;
    private readonly ISqlConnectionFactory _connectionFactory;

    public MemberSelfServiceRepository(IStoredProcedureExecutor sp, ISqlConnectionFactory connectionFactory)
    {
        _sp = sp;
        _connectionFactory = connectionFactory;
    }

    public async Task<MemberGoalDto> CreateGoalAsync(Guid gymId, int memberId, CreateMemberGoalDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@GoalType", dto.GoalType);
        parameters.Add("@TargetValue", dto.TargetValue);
        parameters.Add("@CurrentValue", dto.CurrentValue);
        parameters.Add("@TargetDate", dto.TargetDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@GoalId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var goalId = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateMemberGoal, parameters, "@GoalId", cancellationToken);
        return (await GetGoalByIdAsync(goalId, gymId, memberId, cancellationToken))!;
    }

    public Task UpdateGoalAsync(int goalId, Guid gymId, int memberId, UpdateMemberGoalDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateMemberGoal, new
        {
            GoalId = goalId,
            GymId = gymId,
            MemberId = memberId,
            dto.GoalType,
            dto.TargetValue,
            TargetDate = dto.TargetDate.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);

    public Task UpdateGoalProgressAsync(int goalId, Guid gymId, int memberId, decimal currentValue, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdateMemberGoalProgress, new { GoalId = goalId, GymId = gymId, MemberId = memberId, CurrentValue = currentValue }, cancellationToken);

    public Task CompleteGoalAsync(int goalId, Guid gymId, int memberId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.CompleteMemberGoal, new { GoalId = goalId, GymId = gymId, MemberId = memberId }, cancellationToken);

    public async Task<IReadOnlyList<MemberGoalDto>> GetGoalsAsync(Guid gymId, int memberId, string? status, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberGoalRow>(StoredProcedureNames.GetMemberGoals, new { GymId = gymId, MemberId = memberId, Status = status }, cancellationToken);
        return rows.Select(MapGoal).ToList();
    }

    public async Task<MemberGoalDto?> GetGoalByIdAsync(int goalId, Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<MemberGoalRow>(StoredProcedureNames.GetMemberGoalById, new { GoalId = goalId, GymId = gymId, MemberId = memberId }, cancellationToken);
        return row is null ? null : MapGoal(row);
    }

    public async Task<MemberProgressEntryDto> CreateProgressAsync(Guid gymId, int memberId, Guid? createdBy, CreateMemberProgressDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@Weight", dto.Weight);
        parameters.Add("@BMI", dto.Bmi);
        parameters.Add("@BodyFatPercentage", dto.BodyFatPercentage);
        parameters.Add("@Chest", dto.Chest);
        parameters.Add("@Waist", dto.Waist);
        parameters.Add("@Arms", dto.Arms);
        parameters.Add("@Thighs", dto.Thighs);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@ProgressDate", dto.ProgressDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@ProgressId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        var progressId = await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateMemberProgress, parameters, "@ProgressId", cancellationToken);
        var rows = await _sp.QueryAsync<MemberProgressRow>(StoredProcedureNames.GetMemberProgressHistory, new { GymId = gymId, MemberId = memberId, FromDate = (DateTime?)null, ToDate = (DateTime?)null }, cancellationToken);
        return MapProgress(rows.First(r => r.ProgressId == progressId));
    }

    public async Task<IReadOnlyList<MemberProgressEntryDto>> GetProgressHistoryAsync(Guid gymId, int memberId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberProgressRow>(StoredProcedureNames.GetMemberProgressHistory, new
        {
            GymId = gymId,
            MemberId = memberId,
            FromDate = from?.ToDateTime(TimeOnly.MinValue),
            ToDate = to?.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
        return rows.Select(MapProgress).ToList();
    }

    public async Task<MemberProgressPhotoDto> CreateProgressPhotoAsync(Guid gymId, int memberId, CreateProgressPhotoDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@FileId", dto.FileId);
        parameters.Add("@PhotoType", dto.PhotoType);
        parameters.Add("@ProgressPhotoId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateMemberProgressPhoto, parameters, "@ProgressPhotoId", cancellationToken);
        var photos = await GetProgressPhotosAsync(gymId, memberId, cancellationToken);
        return photos.First();
    }

    public async Task<IReadOnlyList<MemberProgressPhotoDto>> GetProgressPhotosAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberProgressPhotoRow>(StoredProcedureNames.GetMemberProgressPhotos, new { GymId = gymId, MemberId = memberId }, cancellationToken);
        return rows.Select(r => new MemberProgressPhotoDto
        {
            ProgressPhotoId = r.ProgressPhotoId,
            MemberId = r.MemberId,
            FileId = r.FileId,
            PhotoType = r.PhotoType,
            UploadedDate = r.UploadedDate,
            OriginalFileName = r.OriginalFileName,
            ContentType = r.ContentType
        }).ToList();
    }

    public async Task<WaterIntakeDto> UpsertWaterIntakeAsync(Guid gymId, int memberId, UpsertWaterIntakeDto dto, CancellationToken cancellationToken = default)
    {
        var logDate = dto.LogDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@TargetLitres", dto.TargetLitres);
        parameters.Add("@ConsumedLitres", dto.ConsumedLitres);
        parameters.Add("@LogDate", logDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@WaterIntakeId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertWaterIntake, parameters, "@WaterIntakeId", cancellationToken);
        return (await GetWaterIntakeAsync(gymId, memberId, logDate, cancellationToken))!;
    }

    public async Task<WaterIntakeDto?> GetWaterIntakeAsync(Guid gymId, int memberId, DateOnly logDate, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<WaterIntakeRow>(StoredProcedureNames.GetWaterIntake, new { GymId = gymId, MemberId = memberId, LogDate = logDate.ToDateTime(TimeOnly.MinValue) }, cancellationToken);
        return row is null ? null : MapWater(row);
    }

    public async Task<IReadOnlyList<WaterIntakeDto>> GetWaterIntakeHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<WaterIntakeRow>(StoredProcedureNames.GetWaterIntakeHistory, new
        {
            GymId = gymId,
            MemberId = memberId,
            FromDate = from.ToDateTime(TimeOnly.MinValue),
            ToDate = to.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
        return rows.Select(MapWater).ToList();
    }

    public async Task<WorkoutTrackingDto> UpsertWorkoutTrackingAsync(Guid gymId, int memberId, UpsertWorkoutTrackingDto dto, CancellationToken cancellationToken = default)
    {
        var workoutDate = dto.WorkoutDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@WorkoutPlanId", dto.WorkoutPlanId);
        parameters.Add("@ExerciseCompleted", dto.ExerciseCompleted);
        parameters.Add("@CompletionPercentage", dto.CompletionPercentage);
        parameters.Add("@WorkoutDate", workoutDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@WorkoutTrackingId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertWorkoutTracking, parameters, "@WorkoutTrackingId", cancellationToken);
        var history = await GetWorkoutHistoryAsync(gymId, memberId, workoutDate, workoutDate, cancellationToken);
        return history.First();
    }

    public async Task<IReadOnlyList<WorkoutTrackingDto>> GetWorkoutHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<WorkoutTrackingRow>(StoredProcedureNames.GetWorkoutTrackingHistory, new
        {
            GymId = gymId,
            MemberId = memberId,
            FromDate = from.ToDateTime(TimeOnly.MinValue),
            ToDate = to.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
        return rows.Select(MapWorkout).ToList();
    }

    public async Task<int> GetWorkoutStreakAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@StreakDays", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.GetWorkoutStreak, parameters, "@StreakDays", cancellationToken);
        return parameters.Get<int>("@StreakDays");
    }

    public async Task<DietTrackingDto> UpsertDietTrackingAsync(Guid gymId, int memberId, UpsertDietTrackingDto dto, CancellationToken cancellationToken = default)
    {
        var trackingDate = dto.TrackingDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@DietPlanId", dto.DietPlanId);
        parameters.Add("@CompliancePercentage", dto.CompliancePercentage);
        parameters.Add("@MealsCompleted", dto.MealsCompleted);
        parameters.Add("@TrackingDate", trackingDate.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@DietTrackingId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.UpsertDietTracking, parameters, "@DietTrackingId", cancellationToken);
        var history = await GetDietHistoryAsync(gymId, memberId, trackingDate, trackingDate, cancellationToken);
        return history.First();
    }

    public async Task<IReadOnlyList<DietTrackingDto>> GetDietHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<DietTrackingRow>(StoredProcedureNames.GetDietTrackingHistory, new
        {
            GymId = gymId,
            MemberId = memberId,
            FromDate = from.ToDateTime(TimeOnly.MinValue),
            ToDate = to.ToDateTime(TimeOnly.MinValue)
        }, cancellationToken);
        return rows.Select(MapDiet).ToList();
    }

    public async Task<DietComplianceSummaryDto> GetDietComplianceAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@DailyCompliance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
        parameters.Add("@WeeklyCompliance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
        parameters.Add("@MonthlyCompliance", dbType: DbType.Decimal, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.GetDietComplianceSummary, parameters, cancellationToken);
        return new DietComplianceSummaryDto
        {
            DailyCompliance = parameters.Get<decimal>("@DailyCompliance"),
            WeeklyCompliance = parameters.Get<decimal>("@WeeklyCompliance"),
            MonthlyCompliance = parameters.Get<decimal>("@MonthlyCompliance")
        };
    }

    public async Task<string> GetOrCreateReferralCodeAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@ReferralCode", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<string>(StoredProcedureNames.GetOrCreateReferralCode, parameters, "@ReferralCode", cancellationToken);
        return parameters.Get<string>("@ReferralCode") ?? string.Empty;
    }

    public async Task<ReferralDto> RecordReferralConversionAsync(Guid gymId, string referralCode, int referredMemberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@ReferralCode", referralCode);
        parameters.Add("@ReferredMemberId", referredMemberId);
        parameters.Add("@RewardPoints", 100);
        parameters.Add("@ReferralId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.RecordReferralConversion, parameters, "@ReferralId", cancellationToken);
        var stats = await GetReferralStatsAsync(gymId, referredMemberId, cancellationToken);
        return stats.Referrals.First(r => r.ReferralCode == referralCode);
    }

    public async Task<ReferralStatsDto> GetReferralStatsAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var code = await GetOrCreateReferralCodeAsync(gymId, memberId, cancellationToken);
        var referrals = await _sp.QueryAsync<ReferralRow>(StoredProcedureNames.GetReferralStats, new { GymId = gymId, MemberId = memberId }, cancellationToken);
        var list = referrals.Select(r => new ReferralDto
        {
            ReferralId = r.ReferralId,
            ReferralCode = r.ReferralCode,
            ReferredMemberId = r.ReferredMemberId,
            ReferredMemberName = r.ReferredMemberName,
            RewardPoints = r.RewardPoints,
            Status = r.Status,
            CreatedDate = r.CreatedDate
        }).ToList();
        return new ReferralStatsDto
        {
            ReferralCode = code,
            TotalReferrals = list.Count,
            ConvertedReferrals = list.Count(r => r.Status == "Converted"),
            TotalRewardPoints = list.Sum(r => r.RewardPoints),
            Referrals = list
        };
    }

    public async Task<MemberFeedbackDto> CreateFeedbackAsync(Guid gymId, int memberId, CreateMemberFeedbackDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@Rating", dto.Rating);
        parameters.Add("@Comments", dto.Comments);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@FeedbackType", dto.FeedbackType);
        parameters.Add("@FeedbackId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.CreateMemberFeedback, parameters, "@FeedbackId", cancellationToken);
        var feedback = await GetFeedbackAsync(gymId, memberId, cancellationToken);
        return feedback.First();
    }

    public async Task<IReadOnlyList<MemberFeedbackDto>> GetFeedbackAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MemberFeedbackRow>(StoredProcedureNames.GetMemberFeedback, new { GymId = gymId, MemberId = memberId }, cancellationToken);
        return rows.Select(r => new MemberFeedbackDto
        {
            FeedbackId = r.FeedbackId,
            MemberId = r.MemberId,
            Rating = r.Rating,
            Comments = r.Comments,
            TrainerId = r.TrainerId,
            TrainerName = r.TrainerName,
            FeedbackType = r.FeedbackType,
            CreatedDate = r.CreatedDate
        }).ToList();
    }

    public async Task<string> GetOrCreateQrTokenAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@QrToken", dbType: DbType.String, size: 64, direction: ParameterDirection.Output);
        await _sp.ExecuteWithOutputAsync<string>(StoredProcedureNames.GetOrCreateMemberQrToken, parameters, "@QrToken", cancellationToken);
        return parameters.Get<string>("@QrToken") ?? string.Empty;
    }

    public async Task<int> QrCheckInAsync(Guid gymId, int memberId, string qrToken, Guid? markedByUserId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@QrToken", qrToken);
        parameters.Add("@MarkedByUserId", markedByUserId);
        parameters.Add("@MemberAttendanceId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.MemberAttendanceQrCheckIn, parameters, "@MemberAttendanceId", cancellationToken);
    }

    public async Task<MemberSelfServiceDashboardDto> GetDashboardAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetMemberSelfServiceDashboard,
            new { GymId = gymId, MemberId = memberId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var membership = await multi.ReadSingleOrDefaultAsync<DashboardMembershipRow>();
        var attendance = await multi.ReadSingleOrDefaultAsync<DashboardAttendanceRow>();
        var goal = await multi.ReadSingleOrDefaultAsync<DashboardGoalRow>();
        var workout = await multi.ReadSingleOrDefaultAsync<WorkoutTrackingRow>();
        var diet = await multi.ReadSingleOrDefaultAsync<DietTrackingRow>();
        var water = await multi.ReadSingleOrDefaultAsync<WaterIntakeRow>();
        var payments = (await multi.ReadAsync<DashboardPaymentRow>()).ToList();
        var referralSummary = await multi.ReadSingleOrDefaultAsync<DashboardReferralRow>();

        var streak = await GetWorkoutStreakAsync(gymId, memberId, cancellationToken);
        var referralStats = await GetReferralStatsAsync(gymId, memberId, cancellationToken);

        decimal attendancePct = 0;
        if (attendance is not null && attendance.TotalDays > 0)
            attendancePct = Math.Round((decimal)attendance.PresentDays / attendance.TotalDays * 100, 1);

        return new MemberSelfServiceDashboardDto
        {
            ActiveMembership = membership is null ? null : new MemberDashboardMembershipDto
            {
                MembershipId = membership.MembershipId,
                PlanName = membership.PlanName,
                StartDate = DateOnly.FromDateTime(membership.StartDate),
                EndDate = DateOnly.FromDateTime(membership.EndDate),
                Status = membership.Status,
                RemainingDays = membership.RemainingDays
            },
            AttendancePercentage = attendancePct,
            CurrentGoal = goal is null ? null : new MemberGoalDto
            {
                GoalId = goal.GoalId,
                GoalType = goal.GoalType,
                TargetValue = goal.TargetValue,
                CurrentValue = goal.CurrentValue,
                TargetDate = DateOnly.FromDateTime(goal.TargetDate),
                Status = goal.Status,
                ProgressPercent = goal.ProgressPercent
            },
            TodayWorkout = workout is null ? null : MapWorkout(workout),
            TodayDiet = diet is null ? null : MapDiet(diet),
            TodayWater = water is null ? null : MapWater(water),
            RecentPayments = payments.Select(p => new MemberDashboardPaymentDto
            {
                PaymentId = p.PaymentId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                InvoiceNumber = p.InvoiceNumber
            }).ToList(),
            ReferralStats = referralSummary is not null
                ? new ReferralStatsDto
                {
                    ReferralCode = referralStats.ReferralCode,
                    TotalReferrals = referralSummary.TotalReferrals,
                    ConvertedReferrals = referralSummary.ConvertedReferrals,
                    TotalRewardPoints = referralSummary.TotalRewardPoints,
                    Referrals = referralStats.Referrals
                }
                : referralStats,
            WorkoutStreakDays = streak
        };
    }

    public async Task<MemberSelfServiceAnalyticsDto> GetAnalyticsAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            StoredProcedureNames.GetMemberSelfServiceAnalytics,
            new { GymId = gymId, MemberId = memberId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        var goalRow = await multi.ReadSingleOrDefaultAsync<AnalyticsGoalRow>();
        var workoutRow = await multi.ReadSingleOrDefaultAsync<AnalyticsWorkoutRow>();
        var dietCompliance = await multi.ReadSingleOrDefaultAsync<DietComplianceSummaryDto>() ?? new DietComplianceSummaryDto();
        var waterRow = await multi.ReadSingleOrDefaultAsync<AnalyticsWaterRow>();
        var referralRow = await multi.ReadSingleOrDefaultAsync<AnalyticsReferralRow>();

        return new MemberSelfServiceAnalyticsDto
        {
            GoalCompletionRate = goalRow?.GoalCompletionRate ?? 0,
            WorkoutCompliance = workoutRow?.WorkoutCompliance ?? 0,
            DietCompliance = dietCompliance,
            WaterCompliance = waterRow?.WaterCompliance ?? 0,
            ReferralConversion = referralRow?.ReferralConversion ?? 0
        };
    }

    private static MemberGoalDto MapGoal(MemberGoalRow r) => new()
    {
        GoalId = r.GoalId,
        GymId = r.GymId,
        MemberId = r.MemberId,
        GoalType = r.GoalType,
        TargetValue = r.TargetValue,
        CurrentValue = r.CurrentValue,
        TargetDate = DateOnly.FromDateTime(r.TargetDate),
        Status = r.Status,
        CreatedDate = r.CreatedDate,
        UpdatedDate = r.UpdatedDate,
        ProgressPercent = r.TargetValue == 0 ? 0 : Math.Round(r.CurrentValue / r.TargetValue * 100, 1)
    };

    private static MemberProgressEntryDto MapProgress(MemberProgressRow r) => new()
    {
        ProgressId = r.ProgressId,
        GymId = r.GymId,
        MemberId = r.MemberId,
        Weight = r.Weight,
        Bmi = r.BMI,
        BodyFatPercentage = r.BodyFatPercentage,
        Chest = r.Chest,
        Waist = r.Waist,
        Arms = r.Arms,
        Thighs = r.Thighs,
        Notes = r.Notes,
        ProgressDate = DateOnly.FromDateTime(r.ProgressDate),
        CreatedDate = r.CreatedDate
    };

    private static WaterIntakeDto MapWater(WaterIntakeRow r) => new()
    {
        WaterIntakeId = r.WaterIntakeId,
        MemberId = r.MemberId,
        TargetLitres = r.TargetLitres,
        ConsumedLitres = r.ConsumedLitres,
        LogDate = DateOnly.FromDateTime(r.LogDate),
        CompletionPercent = r.TargetLitres == 0 ? 0 : Math.Round(r.ConsumedLitres / r.TargetLitres * 100, 1)
    };

    private static WorkoutTrackingDto MapWorkout(WorkoutTrackingRow r) => new()
    {
        WorkoutTrackingId = r.WorkoutTrackingId,
        MemberId = r.MemberId,
        WorkoutPlanId = r.WorkoutPlanId,
        WorkoutPlanName = r.WorkoutPlanName,
        ExerciseCompleted = r.ExerciseCompleted,
        CompletionPercentage = r.CompletionPercentage,
        WorkoutDate = DateOnly.FromDateTime(r.WorkoutDate)
    };

    private static DietTrackingDto MapDiet(DietTrackingRow r) => new()
    {
        DietTrackingId = r.DietTrackingId,
        MemberId = r.MemberId,
        DietPlanId = r.DietPlanId,
        DietPlanName = r.DietPlanName,
        CompliancePercentage = r.CompliancePercentage,
        MealsCompleted = r.MealsCompleted,
        TrackingDate = DateOnly.FromDateTime(r.TrackingDate)
    };

    private sealed class MemberGoalRow
    {
        public int GoalId { get; set; }
        public Guid GymId { get; set; }
        public int MemberId { get; set; }
        public string GoalType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    private sealed class MemberProgressRow
    {
        public int ProgressId { get; set; }
        public Guid GymId { get; set; }
        public int MemberId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Arms { get; set; }
        public decimal? Thighs { get; set; }
        public string? Notes { get; set; }
        public DateTime ProgressDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    private sealed class MemberProgressPhotoRow
    {
        public int ProgressPhotoId { get; set; }
        public int MemberId { get; set; }
        public long FileId { get; set; }
        public string PhotoType { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
    }

    private sealed class WaterIntakeRow
    {
        public int WaterIntakeId { get; set; }
        public int MemberId { get; set; }
        public decimal TargetLitres { get; set; }
        public decimal ConsumedLitres { get; set; }
        public DateTime LogDate { get; set; }
    }

    private sealed class WorkoutTrackingRow
    {
        public int WorkoutTrackingId { get; set; }
        public int MemberId { get; set; }
        public int WorkoutPlanId { get; set; }
        public string? WorkoutPlanName { get; set; }
        public string? ExerciseCompleted { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime WorkoutDate { get; set; }
    }

    private sealed class DietTrackingRow
    {
        public int DietTrackingId { get; set; }
        public int MemberId { get; set; }
        public int DietPlanId { get; set; }
        public string? DietPlanName { get; set; }
        public decimal CompliancePercentage { get; set; }
        public int MealsCompleted { get; set; }
        public DateTime TrackingDate { get; set; }
    }

    private sealed class ReferralRow
    {
        public int ReferralId { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
        public int? ReferredMemberId { get; set; }
        public string? ReferredMemberName { get; set; }
        public int RewardPoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    private sealed class MemberFeedbackRow
    {
        public int FeedbackId { get; set; }
        public int MemberId { get; set; }
        public int Rating { get; set; }
        public string? Comments { get; set; }
        public int? TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public string FeedbackType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    private sealed class DashboardMembershipRow
    {
        public int MembershipId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RemainingDays { get; set; }
    }

    private sealed class DashboardAttendanceRow
    {
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
    }

    private sealed class DashboardGoalRow
    {
        public int GoalId { get; set; }
        public string GoalType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
    }

    private sealed class DashboardPaymentRow
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
    }

    private sealed class DashboardReferralRow
    {
        public int TotalReferrals { get; set; }
        public int ConvertedReferrals { get; set; }
        public int TotalRewardPoints { get; set; }
    }

    private sealed class AnalyticsGoalRow
    {
        public decimal GoalCompletionRate { get; set; }
    }

    private sealed class AnalyticsWorkoutRow
    {
        public decimal WorkoutCompliance { get; set; }
    }

    private sealed class AnalyticsWaterRow
    {
        public decimal WaterCompliance { get; set; }
    }

    private sealed class AnalyticsReferralRow
    {
        public decimal ReferralConversion { get; set; }
    }
}
