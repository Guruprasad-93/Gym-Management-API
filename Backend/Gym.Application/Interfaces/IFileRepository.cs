using Gym.Application.DTOs.Files;

namespace Gym.Application.Interfaces;

public interface IFileRepository
{
    Task<long> CreateFileAsync(FileDto file, CancellationToken cancellationToken = default);
    Task UpdatePublicUrlAsync(long fileId, Guid gymId, string publicUrl, CancellationToken cancellationToken = default);
    Task<FileDto?> GetFileByIdAsync(long fileId, Guid? gymId, CancellationToken cancellationToken = default);
    Task SoftDeleteFileAsync(long fileId, Guid gymId, CancellationToken cancellationToken = default);
    Task<int> CreateMemberFileAsync(MemberFileDto link, CancellationToken cancellationToken = default);
    Task<int> CreateTrainerFileAsync(TrainerFileDto link, CancellationToken cancellationToken = default);
    Task SetGymLogoAsync(Guid gymId, long fileId, string publicUrl, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberFileDto>> GetMemberFilesAsync(int memberId, Guid? gymId, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerFileDto>> GetTrainerFilesAsync(int trainerId, Guid? gymId, string? category, CancellationToken cancellationToken = default);
    Task<FileDto?> GetGymLogoAsync(Guid gymId, CancellationToken cancellationToken = default);
}
