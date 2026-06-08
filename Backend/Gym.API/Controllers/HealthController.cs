using Gym.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var payload = new
        {
            Status = "Healthy",
            Service = "Gym Management API",
            Timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(payload));
    }
}
