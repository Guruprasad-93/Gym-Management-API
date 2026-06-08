using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services;

public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    public LocalFileStorageProvider(IOptions<FileStorageSettings> settings, IHostEnvironment env)
    {
        _settings = settings.Value;
        _rootPath = Path.IsPathRooted(_settings.LocalRootPath)
            ? _settings.LocalRootPath
            : Path.Combine(env.ContentRootPath, _settings.LocalRootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public string ProviderName => "Local";

    public async Task<string> SaveAsync(
        Guid gymId, string category, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var relative = Path.Combine(gymId.ToString("N"), category, $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}");
        var fullPath = Path.Combine(_rootPath, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, cancellationToken);
        return relative.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored file not found.", fullPath);
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
