using System.Data;
using Dapper;
using Gym.Application.DTOs.Files;
using Gym.Application.Interfaces;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public FileRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<long> CreateFileAsync(FileDto file, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", file.GymId);
        parameters.Add("@FileCategory", file.FileCategory);
        parameters.Add("@StorageProvider", file.StorageProvider);
        parameters.Add("@StoragePath", file.StoragePath);
        parameters.Add("@PublicUrl", file.PublicUrl);
        parameters.Add("@OriginalFileName", file.OriginalFileName);
        parameters.Add("@ContentType", file.ContentType);
        parameters.Add("@FileSizeBytes", file.FileSizeBytes);
        parameters.Add("@Width", file.Width);
        parameters.Add("@Height", file.Height);
        parameters.Add("@UploadedByUserId", file.UploadedByUserId);
        parameters.Add("@FileId", dbType: DbType.Int64, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<long>(StoredProcedureNames.FileCreate, parameters, "@FileId", cancellationToken);
    }

    public Task UpdatePublicUrlAsync(long fileId, Guid gymId, string publicUrl, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.FileUpdatePublicUrl, new { FileId = fileId, GymId = gymId, PublicUrl = publicUrl }, cancellationToken);

    public Task<FileDto?> GetFileByIdAsync(long fileId, Guid? gymId, CancellationToken cancellationToken = default) =>
        _sp.QuerySingleOrDefaultAsync<FileDto>(StoredProcedureNames.FileGetById, new { FileId = fileId, GymId = gymId }, cancellationToken);

    public Task SoftDeleteFileAsync(long fileId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.FileSoftDelete, new { FileId = fileId, GymId = gymId }, cancellationToken);

    public async Task<int> CreateMemberFileAsync(MemberFileDto link, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@MemberId", link.MemberId);
        parameters.Add("@FileId", link.FileId);
        parameters.Add("@GymId", link.GymId);
        parameters.Add("@FileCategory", link.FileCategory);
        parameters.Add("@DietPlanId", link.DietPlanId);
        parameters.Add("@AssignedDietPlanId", link.AssignedDietPlanId);
        parameters.Add("@WorkoutPlanId", link.WorkoutPlanId);
        parameters.Add("@AssignedWorkoutPlanId", link.AssignedWorkoutPlanId);
        parameters.Add("@Notes", link.Notes);
        parameters.Add("@TakenAt", link.TakenAt?.ToDateTime(TimeOnly.MinValue));
        parameters.Add("@MemberFileId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.MemberFileCreate, parameters, "@MemberFileId", cancellationToken);
    }

    public async Task<int> CreateTrainerFileAsync(TrainerFileDto link, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@TrainerId", link.TrainerId);
        parameters.Add("@FileId", link.FileId);
        parameters.Add("@GymId", link.GymId);
        parameters.Add("@FileCategory", link.FileCategory);
        parameters.Add("@TrainerFileId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        return await _sp.ExecuteWithOutputAsync<int>(StoredProcedureNames.TrainerFileCreate, parameters, "@TrainerFileId", cancellationToken);
    }

    public Task SetGymLogoAsync(Guid gymId, long fileId, string publicUrl, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.GymSetLogoFile, new { GymId = gymId, FileId = fileId, PublicUrl = publicUrl }, cancellationToken);

    public async Task<IReadOnlyList<MemberFileDto>> GetMemberFilesAsync(
        int memberId, Guid? gymId, string? category, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<MemberFileDto>(StoredProcedureNames.MemberFilesGetByMember,
            new { MemberId = memberId, GymId = gymId, FileCategory = category }, cancellationToken)).ToList();

    public async Task<IReadOnlyList<TrainerFileDto>> GetTrainerFilesAsync(
        int trainerId, Guid? gymId, string? category, CancellationToken cancellationToken = default) =>
        (await _sp.QueryAsync<TrainerFileDto>(StoredProcedureNames.TrainerFilesGetByTrainer,
            new { TrainerId = trainerId, GymId = gymId, FileCategory = category }, cancellationToken)).ToList();

    public Task<FileDto?> GetGymLogoAsync(Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.QuerySingleOrDefaultAsync<FileDto>(StoredProcedureNames.FileGetGymLogo, new { GymId = gymId }, cancellationToken);
}
