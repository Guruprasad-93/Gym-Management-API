using Gym.Application.DTOs.MemberSelfService;

namespace Gym.Application.Interfaces;

public interface IMemberSelfServiceRepository
{
    Task<MemberGoalDto> CreateGoalAsync(Guid gymId, int memberId, CreateMemberGoalDto dto, CancellationToken cancellationToken = default);
    Task UpdateGoalAsync(int goalId, Guid gymId, int memberId, UpdateMemberGoalDto dto, CancellationToken cancellationToken = default);
    Task UpdateGoalProgressAsync(int goalId, Guid gymId, int memberId, decimal currentValue, CancellationToken cancellationToken = default);
    Task CompleteGoalAsync(int goalId, Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberGoalDto>> GetGoalsAsync(Guid gymId, int memberId, string? status, CancellationToken cancellationToken = default);
    Task<MemberGoalDto?> GetGoalByIdAsync(int goalId, Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<MemberProgressEntryDto> CreateProgressAsync(Guid gymId, int memberId, Guid? createdBy, CreateMemberProgressDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberProgressEntryDto>> GetProgressHistoryAsync(Guid gymId, int memberId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<MemberProgressPhotoDto> CreateProgressPhotoAsync(Guid gymId, int memberId, CreateProgressPhotoDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberProgressPhotoDto>> GetProgressPhotosAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<WaterIntakeDto> UpsertWaterIntakeAsync(Guid gymId, int memberId, UpsertWaterIntakeDto dto, CancellationToken cancellationToken = default);
    Task<WaterIntakeDto?> GetWaterIntakeAsync(Guid gymId, int memberId, DateOnly logDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WaterIntakeDto>> GetWaterIntakeHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<WorkoutTrackingDto> UpsertWorkoutTrackingAsync(Guid gymId, int memberId, UpsertWorkoutTrackingDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutTrackingDto>> GetWorkoutHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<int> GetWorkoutStreakAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<DietTrackingDto> UpsertDietTrackingAsync(Guid gymId, int memberId, UpsertDietTrackingDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DietTrackingDto>> GetDietHistoryAsync(Guid gymId, int memberId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<DietComplianceSummaryDto> GetDietComplianceAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<string> GetOrCreateReferralCodeAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<ReferralDto> RecordReferralConversionAsync(Guid gymId, string referralCode, int referredMemberId, CancellationToken cancellationToken = default);
    Task<ReferralStatsDto> GetReferralStatsAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<MemberFeedbackDto> CreateFeedbackAsync(Guid gymId, int memberId, CreateMemberFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberFeedbackDto>> GetFeedbackAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);

    Task<string> GetOrCreateQrTokenAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<int> QrCheckInAsync(Guid gymId, int memberId, string qrToken, Guid? markedByUserId, CancellationToken cancellationToken = default);

    Task<MemberSelfServiceDashboardDto> GetDashboardAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
    Task<MemberSelfServiceAnalyticsDto> GetAnalyticsAsync(Guid gymId, int memberId, CancellationToken cancellationToken = default);
}

public interface IMemberSelfService
{
    Task<MemberSelfServiceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<MemberSelfServiceAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<ProgressTrendDto> GetProgressTrendsAsync(ProgressQueryDto query, CancellationToken cancellationToken = default);

    Task<MemberGoalDto> CreateGoalAsync(CreateMemberGoalDto dto, CancellationToken cancellationToken = default);
    Task UpdateGoalAsync(int goalId, UpdateMemberGoalDto dto, CancellationToken cancellationToken = default);
    Task UpdateGoalProgressAsync(int goalId, UpdateGoalProgressDto dto, CancellationToken cancellationToken = default);
    Task CompleteGoalAsync(int goalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberGoalDto>> GetGoalsAsync(string? status, CancellationToken cancellationToken = default);

    Task<MemberProgressEntryDto> CreateProgressAsync(CreateMemberProgressDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberProgressEntryDto>> GetProgressHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default);
    Task<MemberProgressPhotoDto> CreateProgressPhotoAsync(CreateProgressPhotoDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberProgressPhotoDto>> GetProgressPhotosAsync(CancellationToken cancellationToken = default);

    Task<WaterIntakeDto> UpsertWaterIntakeAsync(UpsertWaterIntakeDto dto, CancellationToken cancellationToken = default);
    Task<WaterIntakeDto?> GetTodayWaterIntakeAsync(CancellationToken cancellationToken = default);

    Task<WorkoutTrackingDto> UpsertWorkoutTrackingAsync(UpsertWorkoutTrackingDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutTrackingDto>> GetWorkoutHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default);
    Task<int> GetWorkoutStreakAsync(CancellationToken cancellationToken = default);

    Task<DietTrackingDto> UpsertDietTrackingAsync(UpsertDietTrackingDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DietTrackingDto>> GetDietHistoryAsync(ProgressQueryDto query, CancellationToken cancellationToken = default);
    Task<DietComplianceSummaryDto> GetDietComplianceAsync(CancellationToken cancellationToken = default);

    Task<ReferralStatsDto> GetReferralsAsync(CancellationToken cancellationToken = default);

    Task<MemberFeedbackDto> SubmitFeedbackAsync(CreateMemberFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberFeedbackDto>> GetFeedbackAsync(CancellationToken cancellationToken = default);

    Task<MemberQrCodeDto> GetQrCodeAsync(CancellationToken cancellationToken = default);
    Task<QrScanResultDto> ScanQrCheckInAsync(QrCheckInDto dto, CancellationToken cancellationToken = default);

    Task<byte[]> ExportProgressPdfAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportAttendancePdfAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportGoalSummaryPdfAsync(CancellationToken cancellationToken = default);
}

public interface IMemberSelfServiceReportExporter
{
    byte[] ExportProgressPdf(string memberName, IReadOnlyList<MemberProgressEntryDto> entries);
    byte[] ExportAttendancePdf(string memberName, IReadOnlyList<DTOs.Attendance.MemberAttendanceDto> records);
    byte[] ExportGoalSummaryPdf(string memberName, IReadOnlyList<MemberGoalDto> goals);
}
