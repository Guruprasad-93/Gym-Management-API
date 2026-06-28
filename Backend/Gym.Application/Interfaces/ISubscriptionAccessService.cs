using Gym.Application.DTOs.Auth;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Interfaces;

public interface ISubscriptionAccessService
{
    Task<SubscriptionAccessStateDto> ResolveAsync(
        Guid gymId,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default);

    SubscriptionAccessStateDto BuildState(GymSubscriptionDto subscription, IReadOnlyList<string> roles);
}
