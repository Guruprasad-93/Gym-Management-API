namespace Gym.Application.Interfaces;

public interface IFileValidator
{
    void Validate(string fileName, string contentType, long sizeBytes, string category);
}
