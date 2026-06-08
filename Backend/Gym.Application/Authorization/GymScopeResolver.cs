using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Authorization;

public static class GymScopeResolver
{
    public static Guid ResolveRequired(ICurrentUserService user, Guid? requestedGymId = null)
    {
        if (user.HasRole(RoleNames.SuperAdmin))
        {
            if (requestedGymId is null || requestedGymId == Guid.Empty)
                throw new ArgumentException("GymId is required.");
            return requestedGymId.Value;
        }

        var gymId = user.RequireGymId();
        if (requestedGymId.HasValue && requestedGymId.Value != gymId)
            throw new UnauthorizedAccessException("Gym scope mismatch.");
        return gymId;
    }

    public static Guid ResolveForEntity(ICurrentUserService user, Guid entityGymId)
    {
        if (user.HasRole(RoleNames.SuperAdmin))
            return entityGymId;

        var gymId = user.RequireGymId();
        if (entityGymId != gymId)
            throw new KeyNotFoundException("Resource not found.");
        return gymId;
    }
}
