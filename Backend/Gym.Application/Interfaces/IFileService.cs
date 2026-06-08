using Gym.Application.DTOs.Files;

namespace Gym.Application.Interfaces;

public interface IFileService
{
    Task<FileDto> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        UploadFileRequestDto request,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(
        long fileId,
        Guid? signatureGymId,
        long? expiresUnixSeconds,
        string? signature,
        CancellationToken cancellationToken = default);
    Task<FileDto> GetMetadataAsync(long fileId, CancellationToken cancellationToken = default);
    Task DeleteAsync(long fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberFileDto>> GetMemberFilesAsync(int memberId, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainerFileDto>> GetTrainerFilesAsync(int trainerId, string? category, CancellationToken cancellationToken = default);
    Task<FileDto?> GetGymLogoAsync(Guid? gymId, CancellationToken cancellationToken = default);
}
