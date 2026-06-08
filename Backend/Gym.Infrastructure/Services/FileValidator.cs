using Gym.Application.Constants;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services;

public class FileValidator : IFileValidator
{
    private readonly FileStorageSettings _settings;

    public FileValidator(IOptions<FileStorageSettings> settings) => _settings = settings.Value;

    public void Validate(string fileName, string contentType, long sizeBytes, string category)
    {
        if (!FileCategories.All.Contains(category))
            throw new ArgumentException($"Invalid file category: {category}");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
            throw new InvalidOperationException("File must have an extension.");

        var maxSize = FileCategories.IsImageCategory(category) ? _settings.MaxImageSizeBytes : _settings.MaxFileSizeBytes;
        if (sizeBytes <= 0 || sizeBytes > maxSize)
            throw new InvalidOperationException($"File exceeds maximum size of {maxSize / (1024 * 1024)} MB.");

        var allowed = FileCategories.AllowsDocuments(category)
            ? _settings.AllowedImageExtensions.Concat(_settings.AllowedDocumentExtensions)
            : _settings.AllowedImageExtensions;

        if (!allowed.Any(a => a.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"File type {ext} is not allowed for {category}.");
    }
}
