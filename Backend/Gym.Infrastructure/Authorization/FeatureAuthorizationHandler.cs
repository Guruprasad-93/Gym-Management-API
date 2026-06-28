using Gym.Application.Authorization;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Gym.Infrastructure.Authorization;

public class FeatureAuthorizationHandler : AuthorizationHandler<FeatureRequirement>
{
    private readonly IFeatureResolverService _featureResolver;
    private readonly ICurrentUserService _currentUser;

    public FeatureAuthorizationHandler(IFeatureResolverService featureResolver, ICurrentUserService currentUser)
    {
        _featureResolver = featureResolver;
        _currentUser = currentUser;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FeatureRequirement requirement)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (!_currentUser.GymId.HasValue)
            return;

        if (await _featureResolver.HasFeatureAsync(_currentUser.GymId.Value, requirement.FeatureCode))
            context.Succeed(requirement);
    }
}
