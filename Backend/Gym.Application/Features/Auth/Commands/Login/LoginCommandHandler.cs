using Gym.Application.DTOs.Auth;
using Gym.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Gym.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginCommandHandler(IAuthService authService, IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var http = _httpContextAccessor.HttpContext;
        string? userAgent = null;
        if (http?.Request.Headers.TryGetValue("User-Agent", out var ua) == true)
            userAgent = ua.ToString();

        return _authService.LoginAsync(
            new LoginRequestDto
            {
                LoginIdentifier = request.LoginIdentifier,
                Password = request.Password,
                GymId = request.GymId
            },
            userAgent,
            http?.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
    }
}
