using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Options;

namespace Gym.Infrastructure.Services;

public class AzureBlobFileStorageProvider : IFileStorageProvider
{
    private readonly BlobContainerClient _container;

    public AzureBlobFileStorageProvider(IOptions<FileStorageSettings> settings)
    {
        var cfg = settings.Value;
        if (string.IsNullOrWhiteSpace(cfg.AzureConnectionString))
            throw new InvalidOperationException("Azure Blob connection string is not configured.");

        var service = new BlobServiceClient(cfg.AzureConnectionString);
        _container = service.GetBlobContainerClient(cfg.AzureContainerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public string ProviderName => "Azure";

    public async Task<string> SaveAsync(
        Guid gymId, string category, string fileName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var blobName = $"{gymId:N}/{category}/{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var client = _container.GetBlobClient(blobName);
        await client.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
        return blobName;
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var client = _container.GetBlobClient(storagePath);
        if (!await client.ExistsAsync(cancellationToken))
            throw new FileNotFoundException("Blob not found.", storagePath);
        var ms = new MemoryStream();
        await client.DownloadToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        await _container.DeleteBlobIfExistsAsync(storagePath, cancellationToken: cancellationToken);
    }
}
