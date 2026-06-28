using Gym.Application.Services;
using Xunit;

namespace Gym.API.IntegrationTests;

public class SubscriptionRenewalDateTests
{
    private const int GraceDays = 3;

    internal static DateTime ResolvePeriodStart(
        DateTime today,
        DateTime? existingPeriodEnd,
        DateTime? existingGraceEndsAt)
    {
        if (existingPeriodEnd is null)
            return today.Date;

        if (existingGraceEndsAt is not null && today.Date > existingGraceEndsAt.Value.Date)
            return today.Date;

        if (existingGraceEndsAt is null && existingPeriodEnd.Value.Date < today.Date)
            return today.Date;

        return existingPeriodEnd.Value.Date.AddDays(1);
    }

    internal static DateTime ResolvePeriodEnd(DateTime periodStart, string billingCycle) =>
        SaasBillingCycleHelper.CalculatePeriodEnd(periodStart, billingCycle);

    internal static DateTime ResolveGraceEndsAt(DateTime periodEnd, int gracePeriodDays) =>
        periodEnd.AddDays(gracePeriodDays);

    [Theory]
    [MemberData(nameof(EarlyRenewalMonthlyCases))]
    public void EarlyRenewal_Monthly_PreservesRemainingDays(
        DateTime today,
        DateTime currentPeriodEnd,
        DateTime graceEndsAt,
        DateTime expectedStart,
        DateTime expectedEnd,
        DateTime expectedGraceEnd)
    {
        var start = ResolvePeriodStart(today, currentPeriodEnd, graceEndsAt);
        var end = ResolvePeriodEnd(start, "Monthly");
        var grace = ResolveGraceEndsAt(end, GraceDays);

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd, end);
        Assert.Equal(expectedGraceEnd, grace);
    }

    [Theory]
    [MemberData(nameof(GracePeriodRenewalMonthlyCases))]
    public void GracePeriodRenewal_Monthly_ContinuesFromPeriodEnd(
        DateTime today,
        DateTime currentPeriodEnd,
        DateTime graceEndsAt,
        DateTime expectedStart,
        DateTime expectedEnd)
    {
        var start = ResolvePeriodStart(today, currentPeriodEnd, graceEndsAt);
        var end = ResolvePeriodEnd(start, "Monthly");

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [MemberData(nameof(ExpiredRenewalMonthlyCases))]
    public void ExpiredRenewal_Monthly_StartsFromToday(
        DateTime today,
        DateTime currentPeriodEnd,
        DateTime graceEndsAt,
        DateTime expectedStart,
        DateTime expectedEnd)
    {
        var start = ResolvePeriodStart(today, currentPeriodEnd, graceEndsAt);
        var end = ResolvePeriodEnd(start, "Monthly");

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [MemberData(nameof(YearlyRenewalCases))]
    public void YearlyRenewal_UsesOneYearPeriod(
        DateTime today,
        DateTime? currentPeriodEnd,
        DateTime? graceEndsAt,
        DateTime expectedStart,
        DateTime expectedEnd)
    {
        var start = ResolvePeriodStart(today, currentPeriodEnd, graceEndsAt);
        var end = ResolvePeriodEnd(start, "Yearly");

        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [MemberData(nameof(AllCycleFreshStartCases))]
    public void FreshStart_AllBillingCycles_UseCorrectPeriodLength(string billingCycle, DateTime expectedEnd)
    {
        var start = new DateTime(2026, 7, 1);
        var end = ResolvePeriodEnd(start, billingCycle);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [MemberData(nameof(EarlyRenewalAllCyclesCases))]
    public void EarlyRenewal_AllBillingCycles_PreservesRemainingDays(
        string billingCycle,
        DateTime expectedEnd)
    {
        var start = ResolvePeriodStart(
            new DateTime(2026, 6, 10),
            new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 3));
        var end = ResolvePeriodEnd(start, billingCycle);

        Assert.Equal(new DateTime(2026, 7, 1), start);
        Assert.Equal(expectedEnd, end);
    }

    public static IEnumerable<object[]> AllCycleFreshStartCases =>
    [
        ["Monthly", new DateTime(2026, 8, 1)],
        ["Quarterly", new DateTime(2026, 10, 1)],
        ["HalfYearly", new DateTime(2027, 1, 1)],
        ["Yearly", new DateTime(2027, 7, 1)]
    ];

    public static IEnumerable<object[]> EarlyRenewalAllCyclesCases =>
    [
        ["Monthly", new DateTime(2026, 8, 1)],
        ["Quarterly", new DateTime(2026, 10, 1)],
        ["HalfYearly", new DateTime(2027, 1, 1)],
        ["Yearly", new DateTime(2027, 7, 1)]
    ];

    public static IEnumerable<object[]> EarlyRenewalMonthlyCases =>
    [
        [
            new DateTime(2026, 6, 10),
            new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 3),
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 4)
        ],
        [
            new DateTime(2026, 6, 30),
            new DateTime(2026, 6, 30),
            new DateTime(2026, 7, 3),
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            new DateTime(2026, 8, 4)
        ]
    ];

    public static IEnumerable<object[]> GracePeriodRenewalMonthlyCases =>
    [
        [
            new DateTime(2026, 6, 3),
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 4),
            new DateTime(2026, 6, 2),
            new DateTime(2026, 7, 2)
        ],
        [
            new DateTime(2026, 6, 4),
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 4),
            new DateTime(2026, 6, 2),
            new DateTime(2026, 7, 2)
        ]
    ];

    public static IEnumerable<object[]> ExpiredRenewalMonthlyCases =>
    [
        [
            new DateTime(2026, 6, 5),
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 4),
            new DateTime(2026, 6, 5),
            new DateTime(2026, 7, 5)
        ],
        [
            new DateTime(2026, 8, 1),
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 4),
            new DateTime(2026, 8, 1),
            new DateTime(2026, 9, 1)
        ]
    ];

    public static IEnumerable<object[]> YearlyRenewalCases =>
    [
        [
            new DateTime(2026, 3, 15),
            new DateTime(2026, 12, 31),
            new DateTime(2027, 1, 3),
            new DateTime(2027, 1, 1),
            new DateTime(2028, 1, 1)
        ],
        [
            new DateTime(2026, 1, 2),
            new DateTime(2025, 12, 31),
            new DateTime(2026, 1, 3),
            new DateTime(2026, 1, 1),
            new DateTime(2027, 1, 1)
        ],
        [
            new DateTime(2026, 2, 1),
            new DateTime(2025, 12, 31),
            new DateTime(2026, 1, 3),
            new DateTime(2026, 2, 1),
            new DateTime(2027, 2, 1)
        ],
        [
            new DateTime(2026, 5, 10),
            null,
            null,
            new DateTime(2026, 5, 10),
            new DateTime(2027, 5, 10)
        ]
    ];
}
