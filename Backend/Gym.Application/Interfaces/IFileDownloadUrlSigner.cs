namespace Gym.Application.Interfaces;

public interface IFileDownloadUrlSigner
{
    string CreateSignedDownloadUrl(long fileId, Guid gymId);

    bool TryValidate(long fileId, Guid gymId, long expiresUnixSeconds, string signature);
}
