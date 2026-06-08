using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gym.Application.Interfaces;
using Gym.Application.Models;
using Gym.Application.Options;
using Gym.Domain.Constants;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gym.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public string GenerateToken(TokenGenerationContext context)
    {
        var claims = new List<Claim>
        {
            new(AuthClaimTypes.UserId, context.UserId.ToString()),
            new(AuthClaimTypes.FullName, context.FullName),
            new(AuthClaimTypes.Email, context.Email),
            new(AuthClaimTypes.TokenVersion, context.TokenVersion.ToString()),
            new(AuthClaimTypes.SessionId, context.SessionId.ToString()),
            new(ClaimTypes.NameIdentifier, context.UserId.ToString()),
            new(ClaimTypes.Name, context.FullName),
            new(ClaimTypes.Email, context.Email)
        };

        if (context.GymId.HasValue)
            claims.Add(new Claim(AuthClaimTypes.GymId, context.GymId.Value.ToString()));

        foreach (var role in context.Roles)
            claims.Add(new Claim(AuthClaimTypes.Role, role));

        foreach (var permission in context.Permissions)
            claims.Add(new Claim(AuthClaimTypes.Permission, permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
