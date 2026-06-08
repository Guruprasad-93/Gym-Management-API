namespace Gym.Application.Interfaces;

public interface IFileStorageProvider
{
    string ProviderName { get; }
    Task<string> SaveAsync(Guid gymId, string category, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
