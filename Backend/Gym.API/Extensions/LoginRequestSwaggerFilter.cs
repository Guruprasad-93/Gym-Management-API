using Gym.Application.DTOs.Auth;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Gym.API.Extensions;

public sealed class LoginRequestSwaggerFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name != "Login")
            return;

        operation.Description = string.Join(Environment.NewLine, operation.Description, "",
            "Login identifier is globally unique across the platform (no gymId required):",
            "- `superadmin`",
            "- `fitzone_admin`",
            "- `fitzone_trainer1`",
            "- `fitzone_member1`");

        if (operation.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
        {
            media.Example = new OpenApiObject
            {
                ["loginIdentifier"] = new OpenApiString("fitzone_admin"),
                ["password"] = new OpenApiString("Demo@123")
            };
        }
    }
}
