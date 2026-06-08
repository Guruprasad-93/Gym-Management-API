namespace Gym.Application.Options;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = "Local";
    public string LocalRootPath { get; set; } = "uploads";
    public string? AzureConnectionString { get; set; }
    public string AzureContainerName { get; set; } = "gym-files";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxImageSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    public string[] AllowedDocumentExtensions { get; set; } = [".pdf", ".doc", ".docx", ".txt"];
    public int ImageMaxWidth { get; set; } = 1920;
    public int ImageMaxHeight { get; set; } = 1920;
    public int ProfileImageMaxWidth { get; set; } = 512;
    public int ProfileImageMaxHeight { get; set; } = 512;
    public int ImageQuality { get; set; } = 85;
    public int DownloadUrlExpiryMinutes { get; set; } = 60;
    public string? UrlSigningSecret { get; set; }
}
