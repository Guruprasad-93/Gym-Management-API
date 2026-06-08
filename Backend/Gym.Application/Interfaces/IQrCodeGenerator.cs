namespace Gym.Application.Interfaces;

public interface IQrCodeGenerator
{
    string GenerateBase64Png(string payload, int pixelsPerModule = 8);
}
