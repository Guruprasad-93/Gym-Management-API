namespace Gym.Application.DTOs.Files;

public class FileDto
{
    public long FileId { get; set; }
    public Guid GymId { get; set; }
    public string FileCategory { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MemberFileDto
{
    public int MemberFileId { get; set; }
    public int MemberId { get; set; }
    public long FileId { get; set; }
    public Guid GymId { get; set; }
    public string FileCategory { get; set; } = string.Empty;
    public int? DietPlanId { get; set; }
    public int? AssignedDietPlanId { get; set; }
    public int? WorkoutPlanId { get; set; }
    public int? AssignedWorkoutPlanId { get; set; }
    public string? Notes { get; set; }
    public DateOnly? TakenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public class TrainerFileDto
{
    public int TrainerFileId { get; set; }
    public int TrainerId { get; set; }
    public long FileId { get; set; }
    public Guid GymId { get; set; }
    public string FileCategory { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class UploadFileRequestDto
{
    public string FileCategory { get; set; } = string.Empty;
    public Guid? GymId { get; set; }
    public int? MemberId { get; set; }
    public int? TrainerId { get; set; }
    public int? DietPlanId { get; set; }
    public int? AssignedDietPlanId { get; set; }
    public int? WorkoutPlanId { get; set; }
    public int? AssignedWorkoutPlanId { get; set; }
    public string? Notes { get; set; }
    public DateOnly? TakenAt { get; set; }
}

public class StoredFileResult
{
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
