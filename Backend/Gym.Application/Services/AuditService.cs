using System.Text.Json;
using Gym.Application.Authorization;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IAuditLogRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IClientIpProvider _clientIpProvider;

    public AuditService(
        IAuditLogRepository repository,
        ICurrentUserService currentUser,
        IClientIpProvider clientIpProvider)
    {
        _repository = repository;
        _currentUser = currentUser;
        _clientIpProvider = clientIpProvider;
    }

    public async Task LogAsync(AuditLogEntryDto entry, CancellationToken cancellationToken = default)
    {
        var gymId = entry.GymId ?? (_currentUser.HasRole(RoleNames.SuperAdmin) ? null : _currentUser.GymId);
        var userId = entry.UserId ?? _currentUser.UserId;
        var ip = entry.IpAddress ?? _clientIpProvider.GetClientIpAddress();

        await _repository.InsertAsync(
            gymId,
            userId,
            entry.EntityName,
            entry.EntityId,
            entry.ActionType,
            Serialize(entry.OldValue),
            Serialize(entry.NewValue),
            ip,
            cancellationToken);
    }

    public Task LogAuthAsync(string actionType, Guid userId, Guid? gymId, string? ipAddress, CancellationToken cancellationToken = default) =>
        LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            UserId = userId,
            EntityName = Constants.AuditEntityNames.Auth,
            EntityId = userId.ToString(),
            ActionType = actionType,
            IpAddress = ipAddress,
            NewValue = new { userId, gymId, actionType }
        }, cancellationToken);

    public async Task<PagedResultDto<AuditLogDto>> SearchAsync(AuditSearchQueryDto query, CancellationToken cancellationToken = default) =>
        await _repository.SearchAsync(ResolveGymScope(query.GymId), query, cancellationToken);

    public async Task<AuditDashboardDto> GetDashboardAsync(Guid? gymId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default) =>
        await _repository.GetDashboardAsync(ResolveGymScope(gymId), fromDate, toDate, cancellationToken);

    public async Task<IReadOnlyList<AuditLogDto>> GetExportDataAsync(AuditSearchQueryDto query, CancellationToken cancellationToken = default) =>
        await _repository.SearchAllForExportAsync(ResolveGymScope(query.GymId), query, cancellationToken);

    private Guid? ResolveGymScope(Guid? requestedGymId = null)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return requestedGymId is null || requestedGymId == Guid.Empty ? null : requestedGymId;

        return GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
