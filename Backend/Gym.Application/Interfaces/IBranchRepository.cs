using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.Common;

namespace Gym.Application.Interfaces;

public interface IBranchRepository
{
    Task<BranchDto> CreateAsync(Guid gymId, CreateBranchDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int branchId, Guid gymId, UpdateBranchDto dto, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int branchId, Guid gymId, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(int branchId, Guid gymId, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetByIdAsync(int branchId, Guid gymId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<BranchDto>> GetPagedAsync(Guid gymId, BranchSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetAllAsync(Guid gymId, bool includeInactive, CancellationToken cancellationToken = default);
    Task AssignManagerAsync(Guid gymId, int branchId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> TransferMemberAsync(Guid gymId, TransferMemberBranchDto dto, Guid? transferredBy, CancellationToken cancellationToken = default);
    Task<int> TransferTrainerAsync(Guid gymId, TransferTrainerBranchDto dto, Guid? transferredBy, CancellationToken cancellationToken = default);
    Task<PagedResultDto<BranchTransferDto>> GetTransferHistoryAsync(Guid gymId, BranchTransferQueryDto query, CancellationToken cancellationToken = default);
    Task<BranchTargetDto> UpsertTargetAsync(Guid gymId, UpsertBranchTargetDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchTargetDto>> GetTargetsAsync(Guid gymId, int? branchId, DateOnly? targetMonth, CancellationToken cancellationToken = default);
    Task<BranchAnnouncementDto> CreateAnnouncementAsync(Guid gymId, CreateBranchAnnouncementDto dto, Guid? createdBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchAnnouncementDto>> GetAnnouncementsAsync(Guid gymId, int? branchId, string? audience, bool activeOnly, CancellationToken cancellationToken = default);
    Task DeleteAnnouncementAsync(int announcementId, Guid gymId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnnouncementRecipientRow>> GetAnnouncementRecipientsAsync(Guid gymId, int announcementId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDashboardItemDto>> GetDashboardAsync(Guid gymId, int? branchId, CancellationToken cancellationToken = default);
    Task<BranchAnalyticsDto> GetAnalyticsAsync(Guid gymId, int months, CancellationToken cancellationToken = default);
    Task<int> EnsureDefaultBranchAsync(Guid gymId, CancellationToken cancellationToken = default);
}

public record AnnouncementRecipientRow(int EntityId, string? Phone, Guid? RecipientUserId, string? RecipientName);

public interface IBranchService
{
    Task<BranchDto> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int branchId, UpdateBranchDto dto, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int branchId, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(int branchId, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetByIdAsync(int branchId, Guid? gymId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<BranchDto>> GetPagedAsync(BranchSearchQueryDto query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetAllAsync(Guid? gymId, bool includeInactive, CancellationToken cancellationToken = default);
    Task AssignManagerAsync(int branchId, AssignBranchManagerDto dto, CancellationToken cancellationToken = default);
    Task<int> TransferMemberAsync(TransferMemberBranchDto dto, CancellationToken cancellationToken = default);
    Task<int> TransferTrainerAsync(TransferTrainerBranchDto dto, CancellationToken cancellationToken = default);
    Task<PagedResultDto<BranchTransferDto>> GetTransferHistoryAsync(BranchTransferQueryDto query, CancellationToken cancellationToken = default);
    Task<BranchTargetDto> UpsertTargetAsync(UpsertBranchTargetDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchTargetDto>> GetTargetsAsync(int? branchId, DateOnly? targetMonth, CancellationToken cancellationToken = default);
    Task<BranchAnnouncementDto> CreateAnnouncementAsync(CreateBranchAnnouncementDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchAnnouncementDto>> GetAnnouncementsAsync(int? branchId, string? audience, CancellationToken cancellationToken = default);
    Task DeleteAnnouncementAsync(int announcementId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDashboardItemDto>> GetDashboardAsync(int? branchId, CancellationToken cancellationToken = default);
    Task<BranchAnalyticsDto> GetAnalyticsAsync(int months, CancellationToken cancellationToken = default);
}
