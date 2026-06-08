using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<long> InsertAsync(
        Guid? gymId,
        Guid? userId,
        string entityName,
        string entityId,
        string actionType,
        string? oldValueJson = null,
        string? newValueJson = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<AuditLogDto>> SearchAsync(
        Guid? gymId,
        AuditSearchQueryDto query,
        CancellationToken cancellationToken = default);

    Task<AuditDashboardDto> GetDashboardAsync(
        Guid? gymId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> SearchAllForExportAsync(
        Guid? gymId,
        AuditSearchQueryDto query,
        CancellationToken cancellationToken = default);
}
