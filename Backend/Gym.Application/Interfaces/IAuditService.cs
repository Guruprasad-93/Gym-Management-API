using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditLogEntryDto entry, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AuditLogDto>> SearchAsync(AuditSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<AuditDashboardDto> GetDashboardAsync(Guid? gymId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetExportDataAsync(AuditSearchQueryDto query, CancellationToken cancellationToken = default);

    Task LogAuthAsync(string actionType, Guid userId, Guid? gymId, string? ipAddress, CancellationToken cancellationToken = default);
}
