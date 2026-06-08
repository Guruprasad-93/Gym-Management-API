using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Files;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _repository;
    private readonly IFileStorageProvider _storage;
    private readonly IFileValidator _validator;
    private readonly IImageProcessor _imageProcessor;
    private readonly IMemberRepository _memberRepository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFileDownloadUrlSigner _urlSigner;

    public FileService(
        IFileRepository repository,
        IFileStorageProvider storage,
        IFileValidator validator,
        IImageProcessor imageProcessor,
        IMemberRepository memberRepository,
        ITrainerRepository trainerRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFileDownloadUrlSigner urlSigner)
    {
        _repository = repository;
        _storage = storage;
        _validator = validator;
        _imageProcessor = imageProcessor;
        _memberRepository = memberRepository;
        _trainerRepository = trainerRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _urlSigner = urlSigner;
    }

    public async Task<FileDto> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        UploadFileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        EnsureCanUpload(request.FileCategory);
        var gymId = await ResolveGymIdForUploadAsync(request, cancellationToken);
        await EnsureEntityAccessAsync(request, gymId, cancellationToken);

        var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms, cancellationToken);
        var size = ms.Length;
        _validator.Validate(fileName, contentType, size, request.FileCategory);

        ms.Position = 0;
        var isProfile = request.FileCategory is FileCategories.MemberProfilePhoto or FileCategories.TrainerProfilePhoto or FileCategories.GymLogo;
        Stream uploadStream = ms;
        int? width = null, height = null;
        string finalContentType = contentType;
        string finalFileName = fileName;

        if (_imageProcessor.IsImageContentType(contentType))
        {
            var processed = await _imageProcessor.ProcessAsync(ms, contentType, isProfile, cancellationToken);
            uploadStream = processed.Output;
            width = processed.Width;
            height = processed.Height;
            finalContentType = "image/jpeg";
            finalFileName = Path.ChangeExtension(fileName, ".jpg");
            size = uploadStream.Length;
            _validator.Validate(finalFileName, finalContentType, size, request.FileCategory);
        }

        var storagePath = await _storage.SaveAsync(gymId, request.FileCategory, finalFileName, uploadStream, finalContentType, cancellationToken);
        if (uploadStream != ms)
            await uploadStream.DisposeAsync();

        var fileId = await _repository.CreateFileAsync(new FileDto
        {
            GymId = gymId,
            FileCategory = request.FileCategory,
            StorageProvider = _storage.ProviderName,
            StoragePath = storagePath,
            PublicUrl = "/api/files/0/content",
            OriginalFileName = fileName,
            ContentType = finalContentType,
            FileSizeBytes = size,
            Width = width,
            Height = height,
            UploadedByUserId = _currentUser.UserId
        }, cancellationToken);

        var publicUrl = _urlSigner.CreateSignedDownloadUrl(fileId, gymId);
        await _repository.UpdatePublicUrlAsync(fileId, gymId, publicUrl, cancellationToken);
        await LinkFileAsync(request, gymId, fileId, publicUrl, cancellationToken);

        var created = (await _repository.GetFileByIdAsync(fileId, gymId, cancellationToken))!;
        created.PublicUrl = publicUrl;

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.File,
            EntityId = fileId.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = new { request.FileCategory, created.PublicUrl, fileName }
        }, cancellationToken);

        return created;
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAsync(
        long fileId,
        Guid? signatureGymId,
        long? expiresUnixSeconds,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        FileDto file;

        if (signatureGymId.HasValue
            && expiresUnixSeconds.HasValue
            && !string.IsNullOrWhiteSpace(signature)
            && _urlSigner.TryValidate(fileId, signatureGymId.Value, expiresUnixSeconds.Value, signature))
        {
            file = await _repository.GetFileByIdAsync(fileId, signatureGymId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("File not found.");
        }
        else if (_currentUser.IsAuthenticated)
        {
            file = await GetMetadataAsync(fileId, cancellationToken);
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid or expired download link.");
        }

        var stream = await _storage.OpenReadAsync(file.StoragePath, cancellationToken);
        return (stream, file.ContentType, file.OriginalFileName);
    }

    public async Task<FileDto> GetMetadataAsync(long fileId, CancellationToken cancellationToken = default) =>
        await _repository.GetFileByIdAsync(fileId, ResolveGymScope(), cancellationToken)
        ?? throw new KeyNotFoundException("File not found.");

    public async Task DeleteAsync(long fileId, CancellationToken cancellationToken = default)
    {
        EnsureCanDelete();
        var file = await GetMetadataAsync(fileId, cancellationToken);
        await _repository.SoftDeleteFileAsync(fileId, file.GymId, cancellationToken);
        try { await _storage.DeleteAsync(file.StoragePath, cancellationToken); } catch { /* best effort */ }
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = file.GymId,
            EntityName = AuditEntityNames.File,
            EntityId = fileId.ToString(),
            ActionType = AuditActionTypes.Delete,
            OldValue = file
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberFileDto>> GetMemberFilesAsync(
        int memberId, string? category, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAccessAsync(memberId, cancellationToken);
        var gymId = await ResolveGymIdForMemberAsync(memberId, cancellationToken);
        var files = await _repository.GetMemberFilesAsync(memberId, gymId, category, cancellationToken);
        foreach (var f in files)
            f.PublicUrl = _urlSigner.CreateSignedDownloadUrl(f.FileId, gymId);
        return files;
    }

    public async Task<IReadOnlyList<TrainerFileDto>> GetTrainerFilesAsync(
        int trainerId, string? category, CancellationToken cancellationToken = default)
    {
        await EnsureTrainerAccessAsync(trainerId, cancellationToken);
        var gymId = ResolveGymScope();
        var files = await _repository.GetTrainerFilesAsync(trainerId, gymId, category, cancellationToken);
        foreach (var f in files)
            f.PublicUrl = _urlSigner.CreateSignedDownloadUrl(f.FileId, gymId);
        return files;
    }

    public async Task<FileDto?> GetGymLogoAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var id = gymId ?? (_currentUser.HasRole(RoleNames.SuperAdmin) ? throw new ArgumentException("GymId required.") : _currentUser.RequireGymId());
        var file = await _repository.GetGymLogoAsync(id, cancellationToken);
        if (file is not null)
            file.PublicUrl = _urlSigner.CreateSignedDownloadUrl(file.FileId, id);
        return file;
    }

    private async Task LinkFileAsync(
        UploadFileRequestDto request, Guid gymId, long fileId, string publicUrl, CancellationToken cancellationToken)
    {
        switch (request.FileCategory)
        {
            case FileCategories.GymLogo:
                await _repository.SetGymLogoAsync(gymId, fileId, publicUrl, cancellationToken);
                break;
            case FileCategories.MemberProfilePhoto:
            case FileCategories.MemberProgressPhoto:
            case FileCategories.DietAttachment:
            case FileCategories.WorkoutAttachment:
                if (request.MemberId is null)
                    throw new ArgumentException("MemberId is required.");
                await _repository.CreateMemberFileAsync(new MemberFileDto
                {
                    MemberId = request.MemberId.Value,
                    FileId = fileId,
                    GymId = gymId,
                    FileCategory = request.FileCategory,
                    DietPlanId = request.DietPlanId,
                    AssignedDietPlanId = request.AssignedDietPlanId,
                    WorkoutPlanId = request.WorkoutPlanId,
                    AssignedWorkoutPlanId = request.AssignedWorkoutPlanId,
                    Notes = request.Notes,
                    TakenAt = request.TakenAt
                }, cancellationToken);
                break;
            case FileCategories.TrainerProfilePhoto:
                if (request.TrainerId is null)
                    throw new ArgumentException("TrainerId is required.");
                await _repository.CreateTrainerFileAsync(new TrainerFileDto
                {
                    TrainerId = request.TrainerId.Value,
                    FileId = fileId,
                    GymId = gymId,
                    FileCategory = request.FileCategory
                }, cancellationToken);
                break;
        }
    }

    private async Task<Guid> ResolveGymIdForUploadAsync(UploadFileRequestDto request, CancellationToken cancellationToken)
    {
        if (request.FileCategory == FileCategories.GymLogo)
        {
            if (_currentUser.HasRole(RoleNames.SuperAdmin))
            {
                if (request.GymId is null) throw new ArgumentException("GymId is required for logo upload.");
                return request.GymId.Value;
            }
            return _currentUser.RequireGymId();
        }

        if (request.MemberId.HasValue)
        {
            var memberGymId = _currentUser.HasRole(RoleNames.SuperAdmin)
                ? await _memberRepository.GetGymIdAsync(request.MemberId.Value, cancellationToken)
                    ?? throw new KeyNotFoundException("Member not found.")
                : _currentUser.RequireGymId();
            var member = await _memberRepository.GetByIdAsync(request.MemberId.Value, memberGymId, null, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            return member.GymId;
        }

        if (request.TrainerId.HasValue)
        {
            var trainerGymId = ResolveGymScope(request.GymId);
            var trainer = await _trainerRepository.GetByIdAsync(request.TrainerId.Value, trainerGymId, cancellationToken)
                ?? throw new KeyNotFoundException("Trainer not found.");
            return trainer.GymId;
        }

        return _currentUser.HasRole(RoleNames.SuperAdmin)
            ? request.GymId ?? throw new ArgumentException("GymId is required.")
            : _currentUser.RequireGymId();
    }

    private async Task EnsureEntityAccessAsync(UploadFileRequestDto request, Guid gymId, CancellationToken cancellationToken)
    {
        if (request.MemberId.HasValue)
            await EnsureMemberAccessAsync(request.MemberId.Value, cancellationToken);
        if (request.TrainerId.HasValue)
            await EnsureTrainerAccessAsync(request.TrainerId.Value, cancellationToken);
    }

    private async Task EnsureMemberAccessAsync(int memberId, CancellationToken cancellationToken)
    {
        if (IsMemberOnly())
        {
            var own = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");
            if (own.Id != memberId) throw new KeyNotFoundException("Member not found.");
            return;
        }
        int? trainerFilter = IsTrainerOnly() ? await GetTrainerIdAsync(cancellationToken) : null;
        var gymId = await ResolveGymIdForMemberAsync(memberId, cancellationToken);
        _ = await _memberRepository.GetByIdAsync(memberId, gymId, trainerFilter, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");
    }

    private async Task EnsureTrainerAccessAsync(int trainerId, CancellationToken cancellationToken)
    {
        if (IsTrainerOnly())
        {
            var own = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new UnauthorizedAccessException("Trainer not found.");
            if (own.Id != trainerId) throw new UnauthorizedAccessException("Access denied.");
            return;
        }
        _ = await _trainerRepository.GetByIdAsync(trainerId, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Trainer not found.");
    }

    private async Task<int?> GetTrainerIdAsync(CancellationToken cancellationToken)
    {
        var t = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken);
        return t?.Id;
    }

    private void EnsureCanUpload(string category)
    {
        if (_currentUser.HasPermission(Permissions.UploadFiles) || _currentUser.HasPermission(Permissions.ManageFiles))
            return;
        if (IsMemberOnly() && category is FileCategories.MemberProfilePhoto or FileCategories.MemberProgressPhoto)
            return;
        throw new UnauthorizedAccessException("Insufficient permission to upload files.");
    }

    private void EnsureCanDelete()
    {
        if (!_currentUser.HasPermission(Permissions.DeleteFiles) && !_currentUser.HasPermission(Permissions.ManageFiles))
            throw new UnauthorizedAccessException("Insufficient permission to delete files.");
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole(RoleNames.Trainer) && !_currentUser.HasRole(RoleNames.GymAdmin) && !_currentUser.HasRole(RoleNames.SuperAdmin);

    private bool IsMemberOnly() =>
        _currentUser.HasRole(RoleNames.Member) && !_currentUser.HasRole(RoleNames.GymAdmin) && !_currentUser.HasRole(RoleNames.SuperAdmin) && !_currentUser.HasRole(RoleNames.Trainer);

    private async Task<Guid> ResolveGymIdForMemberAsync(int memberId, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasRole(RoleNames.SuperAdmin))
            return _currentUser.RequireGymId();

        var gymId = await _memberRepository.GetGymIdAsync(memberId, cancellationToken);
        return gymId ?? throw new KeyNotFoundException("Member not found.");
    }

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
}
