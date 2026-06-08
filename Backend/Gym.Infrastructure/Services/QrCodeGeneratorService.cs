using Gym.Application.Interfaces;
using QRCoder;

namespace Gym.Infrastructure.Services;

public class QrCodeGeneratorService : IQrCodeGenerator
{
    public string GenerateBase64Png(string payload, int pixelsPerModule = 8)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
        var bytes = qrCode.GetGraphic(pixelsPerModule);
        return Convert.ToBase64String(bytes);
    }
}
