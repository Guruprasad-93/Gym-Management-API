using Gym.Application.Models;

namespace Gym.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(TokenGenerationContext context);
}
