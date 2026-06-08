namespace Gym.Application.DTOs.Audit;

public class AuditLogDto
{
    public long AuditLogId { get; set; }
    public Guid? GymId { get; set; }
    public string? GymName { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AuditSearchQueryDto
{
    public Guid? GymId { get; set; }
    public Guid? UserId { get; set; }
    public string? EntityName { get; set; }
    public string? ActionType { get; set; }
    public string? EntityId { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuditDashboardDto
{
    public int TotalLogs { get; set; }
    public IReadOnlyList<AuditCountByKeyDto> ByEntity { get; set; } = [];
    public IReadOnlyList<AuditCountByKeyDto> ByAction { get; set; } = [];
}

public class AuditCountByKeyDto
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AuditLogEntryDto
{
    public Guid? GymId { get; set; }
    public Guid? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public string? IpAddress { get; set; }
}
