using Gym.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Gym.Infrastructure.Services;

public class ClientIpProvider : IClientIpProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientIpProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string? GetClientIpAddress() =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
