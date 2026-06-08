using Gym.Application.Interfaces;
using Gym.Application.Options;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Gym.Infrastructure.Services;

public class ImageProcessor : IImageProcessor
{
    private readonly FileStorageSettings _settings;

    public ImageProcessor(IOptions<FileStorageSettings> settings) => _settings = settings.Value;

    public bool IsImageContentType(string contentType) =>
        contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public async Task<(Stream Output, int? Width, int? Height)> ProcessAsync(
        Stream input,
        string contentType,
        bool isProfileImage,
        CancellationToken cancellationToken = default)
    {
        if (!IsImageContentType(contentType))
            return (input, null, null);

        using var image = await Image.LoadAsync(input, cancellationToken);
        var maxW = isProfileImage ? _settings.ProfileImageMaxWidth : _settings.ImageMaxWidth;
        var maxH = isProfileImage ? _settings.ProfileImageMaxHeight : _settings.ImageMaxHeight;

        if (image.Width > maxW || image.Height > maxH)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxW, maxH)
            }));
        }

        var output = new MemoryStream();
        var encoder = new JpegEncoder { Quality = _settings.ImageQuality };
        await image.SaveAsJpegAsync(output, encoder, cancellationToken);
        output.Position = 0;
        return (output, image.Width, image.Height);
    }
}
