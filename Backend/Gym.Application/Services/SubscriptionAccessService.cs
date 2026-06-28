using Gym.Application.Constants;

using Gym.Application.DTOs.Auth;

using Gym.Application.DTOs.Saas;

using Gym.Application.Interfaces;

using Gym.Domain.Constants;



namespace Gym.Application.Services;



public class SubscriptionAccessService : ISubscriptionAccessService

{

    private readonly ISaasSubscriptionRepository _saasRepository;



    public SubscriptionAccessService(ISaasSubscriptionRepository saasRepository) =>

        _saasRepository = saasRepository;



    public async Task<SubscriptionAccessStateDto> ResolveAsync(

        Guid gymId,

        IReadOnlyList<string> roles,

        CancellationToken cancellationToken = default)

    {

        var subscription = await _saasRepository.GetGymSubscriptionAsync(gymId, cancellationToken);

        if (subscription is null)

        {

            return new SubscriptionAccessStateDto

            {

                AccessMode = SubscriptionAccessModes.Active,

                HasSubscriptionAccess = true

            };

        }



        return BuildState(subscription, roles);

    }



    public SubscriptionAccessStateDto BuildState(GymSubscriptionDto subscription, IReadOnlyList<string> roles)

    {

        var now = DateTime.UtcNow;

        var today = now.Date;

        var daysToExpiry = SubscriptionExpiryCalculator.ComputeDaysToExpiry(today, subscription.CurrentPeriodEnd);

        var graceDaysRemaining = SubscriptionExpiryCalculator.ComputeGraceDaysRemaining(today, subscription.GraceEndsAt);



        if (subscription.HasAccess)

        {

            if (SubscriptionExpiryCalculator.IsInGracePeriod(subscription, today))

            {

                var state = new SubscriptionAccessStateDto

                {

                    AccessMode = SubscriptionAccessModes.GracePeriod,

                    HasSubscriptionAccess = true,

                    GraceEndsAt = subscription.GraceEndsAt,

                    GraceDaysRemaining = graceDaysRemaining,

                    DaysToExpiry = 0

                };

                ApplyBanner(state, subscription);

                return state;

            }



            var activeState = new SubscriptionAccessStateDto

            {

                AccessMode = SubscriptionAccessModes.Active,

                HasSubscriptionAccess = true,

                GraceEndsAt = subscription.GraceEndsAt,

                DaysToExpiry = daysToExpiry,

                GraceDaysRemaining = graceDaysRemaining

            };

            ApplyBanner(activeState, subscription);

            return activeState;

        }



        var isGymAdmin = roles.Contains(RoleNames.GymAdmin, StringComparer.OrdinalIgnoreCase);

        return new SubscriptionAccessStateDto

        {

            AccessMode = isGymAdmin

                ? SubscriptionAccessModes.ExpiredAdminRenewal

                : SubscriptionAccessModes.ExpiredLocked,

            HasSubscriptionAccess = false,

            GraceEndsAt = subscription.GraceEndsAt,

            DaysToExpiry = 0,

            GraceDaysRemaining = 0

        };

    }



    private static void ApplyBanner(SubscriptionAccessStateDto state, GymSubscriptionDto subscription)

    {

        state.BannerMessage = SubscriptionExpiryCalculator.ResolveBannerMessage(

            state.AccessMode,

            state.DaysToExpiry,

            state.GraceDaysRemaining,

            subscription.GraceEndsAt,

            DateTime.UtcNow.Date);

        state.BannerSeverity = SubscriptionExpiryCalculator.ResolveBannerPriority(

            state.AccessMode,

            state.DaysToExpiry,

            state.GraceDaysRemaining);

    }

}


