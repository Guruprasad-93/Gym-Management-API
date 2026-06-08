namespace Gym.Application.DTOs.Branches;

public class BranchDto
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

public class CreateBranchDto
{
    public Guid? GymId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? ManagerUserId { get; set; }
}

public class UpdateBranchDto
{
    public string BranchName { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class AssignBranchManagerDto
{
    public Guid UserId { get; set; }
}

public class BranchSearchQueryDto : Common.PagedRequestDto
{
    public Guid? GymId { get; set; }
    public bool IncludeInactive { get; set; }
}

public class BranchTransferDto
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

public class TransferMemberBranchDto
{
    public int MemberId { get; set; }
    public int ToBranchId { get; set; }
    public string? Notes { get; set; }
}

public class TransferTrainerBranchDto
{
    public int TrainerId { get; set; }
    public int ToBranchId { get; set; }
    public string? Notes { get; set; }
}

public class BranchTransferQueryDto : Common.PagedRequestDto
{
    public Guid? GymId { get; set; }
    public string? EntityType { get; set; }
    public int? BranchId { get; set; }
}

public class BranchTargetDto
{
    public int TargetId { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateOnly TargetMonth { get; set; }
    public decimal RevenueTarget { get; set; }
    public int NewMembersTarget { get; set; }
    public int LeadConversionsTarget { get; set; }
    public decimal ActualRevenue { get; set; }
    public int ActualNewMembers { get; set; }
    public int ActualLeadConversions { get; set; }
    public decimal RevenueAchievementPercent { get; set; }
    public decimal MembersAchievementPercent { get; set; }
    public decimal LeadsAchievementPercent { get; set; }
}

public class UpsertBranchTargetDto
{
    public int BranchId { get; set; }
    public DateOnly TargetMonth { get; set; }
    public decimal RevenueTarget { get; set; }
    public int NewMembersTarget { get; set; }
    public int LeadConversionsTarget { get; set; }
}

public class BranchAnnouncementDto
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

public class CreateBranchAnnouncementDto
{
    public int? BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = "All";
    public DateTime? ExpiryDate { get; set; }
    public bool SendWhatsApp { get; set; }
}

public class BranchDashboardItemDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int TrainerCount { get; set; }
    public decimal RevenueMonth { get; set; }
    public int AttendanceMonth { get; set; }
    public int LeadsOpen { get; set; }
    public decimal ExpensesMonth { get; set; }
    public decimal ProfitMonth => RevenueMonth - ExpensesMonth;
}

public class BranchAnalyticsRankingDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Profit => TotalRevenue - TotalExpenses;
    public int MemberCount { get; set; }
    public int AttendanceCount { get; set; }
    public int LeadConversions { get; set; }
}

public class BranchMonthlyRevenueDto
{
    public string BranchName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

public class BranchAnalyticsDto
{
    public IReadOnlyList<BranchAnalyticsRankingDto> Rankings { get; set; } = Array.Empty<BranchAnalyticsRankingDto>();
    public IReadOnlyList<BranchMonthlyRevenueDto> MonthlyRevenue { get; set; } = Array.Empty<BranchMonthlyRevenueDto>();
}

public static class BranchEntityTypes
{
    public const string Member = "Member";
    public const string Trainer = "Trainer";
}

public static class BranchAnnouncementAudiences
{
    public const string All = "All";
    public const string Staff = "Staff";
    public const string Members = "Members";
}
