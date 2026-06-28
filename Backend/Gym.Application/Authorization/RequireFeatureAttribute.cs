using Gym.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Gym.Application.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireFeatureAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "FEATURE:";

    public RequireFeatureAttribute(string featureCode)
    {
        Policy = PolicyPrefix + featureCode;
    }
}

public sealed class FeatureRequirement : IAuthorizationRequirement
{
    public FeatureRequirement(string featureCode) => FeatureCode = featureCode;
    public string FeatureCode { get; }
}
