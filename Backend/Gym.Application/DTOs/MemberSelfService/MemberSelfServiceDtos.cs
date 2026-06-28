namespace Gym.Application.DTOs.MemberSelfService;

public static class GoalTypes
{
    public const string WeightLoss = "WeightLoss";
    public const string WeightGain = "WeightGain";
    public const string MuscleGain = "MuscleGain";
    public const string FatLoss = "FatLoss";
}

public static class GoalStatuses
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class FeedbackTypes
{
    public const string Trainer = "Trainer";
    public const string Gym = "Gym";
}

public class MemberGoalDto
{
    public int GoalId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public string GoalType { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public DateOnly TargetDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public decimal ProgressPercent { get; set; }
}

public class CreateMemberGoalDto
{
    public string GoalType { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public DateOnly TargetDate { get; set; }
}

public class UpdateMemberGoalDto
{
    public string GoalType { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public DateOnly TargetDate { get; set; }
}

public class UpdateGoalProgressDto
{
    public decimal CurrentValue { get; set; }
}

public class MemberProgressEntryDto
{
    public int ProgressId { get; set; }
    public Guid GymId { get; set; }
    public int MemberId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Bmi { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Arms { get; set; }
    public decimal? Thighs { get; set; }
    public string? Notes { get; set; }
    public DateOnly ProgressDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateMemberProgressDto
{
    public decimal? Weight { get; set; }
    public decimal? Bmi { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Arms { get; set; }
    public decimal? Thighs { get; set; }
    public string? Notes { get; set; }
    public DateOnly ProgressDate { get; set; }
}

public class MemberProgressPhotoDto
{
    public int ProgressPhotoId { get; set; }
    public int MemberId { get; set; }
    public long FileId { get; set; }
    public string PhotoType { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
}

public class CreateProgressPhotoDto
{
    public long FileId { get; set; }
    public string PhotoType { get; set; } = "Front";
}

public class WaterIntakeDto
{
    public int WaterIntakeId { get; set; }
    public int MemberId { get; set; }
    public decimal TargetLitres { get; set; }
    public decimal ConsumedLitres { get; set; }
    public DateOnly LogDate { get; set; }
    public decimal CompletionPercent { get; set; }
}

public class UpsertWaterIntakeDto
{
    public decimal TargetLitres { get; set; } = 2.5m;
    public decimal ConsumedLitres { get; set; }
    public DateOnly? LogDate { get; set; }
}

public class WorkoutTrackingDto
{
    public int WorkoutTrackingId { get; set; }
    public int MemberId { get; set; }
    public int WorkoutPlanId { get; set; }
    public string? WorkoutPlanName { get; set; }
    public string? ExerciseCompleted { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateOnly WorkoutDate { get; set; }
}

public class UpsertWorkoutTrackingDto
{
    public int WorkoutPlanId { get; set; }
    public string? ExerciseCompleted { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateOnly? WorkoutDate { get; set; }
}

public class DietTrackingDto
{
    public int DietTrackingId { get; set; }
    public int MemberId { get; set; }
    public int DietPlanId { get; set; }
    public string? DietPlanName { get; set; }
    public decimal CompliancePercentage { get; set; }
    public int MealsCompleted { get; set; }
    public DateOnly TrackingDate { get; set; }
}

public class UpsertDietTrackingDto
{
    public int DietPlanId { get; set; }
    public decimal CompliancePercentage { get; set; }
    public int MealsCompleted { get; set; }
    public DateOnly? TrackingDate { get; set; }
}

public class DietComplianceSummaryDto
{
    public decimal DailyCompliance { get; set; }
    public decimal WeeklyCompliance { get; set; }
    public decimal MonthlyCompliance { get; set; }
}

public class ReferralDto
{
    public int ReferralId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public int? ReferredMemberId { get; set; }
    public string? ReferredMemberName { get; set; }
    public int RewardPoints { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class ReferralStatsDto
{
    public string ReferralCode { get; set; } = string.Empty;
    public int TotalReferrals { get; set; }
    public int ConvertedReferrals { get; set; }
    public int TotalRewardPoints { get; set; }
    public IReadOnlyList<ReferralDto> Referrals { get; set; } = Array.Empty<ReferralDto>();
}

public class MemberFeedbackDto
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

public class CreateMemberFeedbackDto
{
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public int? TrainerId { get; set; }
    public string FeedbackType { get; set; } = FeedbackTypes.Gym;
}

public class MemberQrCodeDto
{
    public string Payload { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public int MemberId { get; set; }
}

public class QrCheckInDto
{
    public string QrPayload { get; set; } = string.Empty;
}

public class QrScanResultDto
{
    public int? AttendanceId { get; set; }
    public int? BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string? MembershipStatus { get; set; }
    public string? MembershipPlanName { get; set; }
    public string? BookingStatus { get; set; }
    public string? ClassName { get; set; }
}

public class MemberDashboardMembershipDto
{
    public int MembershipId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RemainingDays { get; set; }
}

public class MemberDashboardPaymentDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
}

public class MemberTodayVisitDto
{
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string? CheckoutType { get; set; }
    public bool IsAutoCheckout { get; set; }
    public bool IsCurrentlyCheckedIn { get; set; }
    public string? CheckedOutByName { get; set; }
}

public class MemberSelfServiceDashboardDto
{
    public MemberDashboardMembershipDto? ActiveMembership { get; set; }
    public MemberTodayVisitDto? TodayVisit { get; set; }
    public decimal AttendancePercentage { get; set; }
    public MemberGoalDto? CurrentGoal { get; set; }
    public WorkoutTrackingDto? TodayWorkout { get; set; }
    public DietTrackingDto? TodayDiet { get; set; }
    public WaterIntakeDto? TodayWater { get; set; }
    public IReadOnlyList<MemberDashboardPaymentDto> RecentPayments { get; set; } = Array.Empty<MemberDashboardPaymentDto>();
    public ReferralStatsDto ReferralStats { get; set; } = new();
    public int WorkoutStreakDays { get; set; }
}

public class MemberSelfServiceAnalyticsDto
{
    public decimal GoalCompletionRate { get; set; }
    public decimal WorkoutCompliance { get; set; }
    public DietComplianceSummaryDto DietCompliance { get; set; } = new();
    public decimal WaterCompliance { get; set; }
    public decimal ReferralConversion { get; set; }
}

public class ProgressTrendDto
{
    public IReadOnlyList<MemberProgressEntryDto> Entries { get; set; } = Array.Empty<MemberProgressEntryDto>();
    public IReadOnlyList<WaterIntakeDto> WaterHistory { get; set; } = Array.Empty<WaterIntakeDto>();
    public IReadOnlyList<WorkoutTrackingDto> WorkoutHistory { get; set; } = Array.Empty<WorkoutTrackingDto>();
    public IReadOnlyList<DietTrackingDto> DietHistory { get; set; } = Array.Empty<DietTrackingDto>();
}

public class ProgressQueryDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
