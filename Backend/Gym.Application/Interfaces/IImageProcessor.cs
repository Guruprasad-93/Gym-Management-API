namespace Gym.Application.Interfaces;

public interface IImageProcessor
{
    bool IsImageContentType(string contentType);
    Task<(Stream Output, int? Width, int? Height)> ProcessAsync(
        Stream input,
        string contentType,
        bool isProfileImage,
        CancellationToken cancellationToken = default);
}
