using Gym.Application.Constants;
using Gym.Application.DTOs.Saas;

namespace Gym.Application.Services;

public static class SaasBillingCycleHelper
{
    public static readonly string[] PaidCycles =
    [
        SaasBillingCycles.Monthly,
        SaasBillingCycles.Quarterly,
        SaasBillingCycles.HalfYearly,
        SaasBillingCycles.Yearly
    ];

    public static string Normalize(string billingCycle)
    {
        if (string.IsNullOrWhiteSpace(billingCycle))
            return SaasBillingCycles.Monthly;

        return billingCycle.Trim().ToUpperInvariant() switch
        {
            "MONTHLY" => SaasBillingCycles.Monthly,
            "QUARTERLY" => SaasBillingCycles.Quarterly,
            "HALFYEARLY" or "HALF-YEARLY" or "HALF YEARLY" or "HALF_YEARLY" => SaasBillingCycles.HalfYearly,
            "YEARLY" => SaasBillingCycles.Yearly,
            _ => throw new ArgumentException($"Unsupported billing cycle '{billingCycle}'.")
        };
    }

    public static bool IsValidPaidCycle(string billingCycle)
    {
        try
        {
            _ = Normalize(billingCycle);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static DateTime CalculatePeriodEnd(DateTime periodStart, string billingCycle)
    {
        var normalized = Normalize(billingCycle);
        return normalized switch
        {
            SaasBillingCycles.Monthly => periodStart.AddMonths(1),
            SaasBillingCycles.Quarterly => periodStart.AddMonths(3),
            SaasBillingCycles.HalfYearly => periodStart.AddMonths(6),
            SaasBillingCycles.Yearly => periodStart.AddYears(1),
            _ => periodStart.AddMonths(1)
        };
    }

    public static decimal ResolvePrice(SaasPlanDto plan, string billingCycle)
    {
        var normalized = Normalize(billingCycle);
        return normalized switch
        {
            SaasBillingCycles.Monthly => plan.MonthlyPrice,
            SaasBillingCycles.Quarterly => plan.QuarterlyPrice,
            SaasBillingCycles.HalfYearly => plan.HalfYearlyPrice,
            SaasBillingCycles.Yearly => plan.YearlyPrice,
            _ => plan.MonthlyPrice
        };
    }

    public static int GetDurationMonths(string billingCycle) =>
        Normalize(billingCycle) switch
        {
            SaasBillingCycles.Monthly => 1,
            SaasBillingCycles.Quarterly => 3,
            SaasBillingCycles.HalfYearly => 6,
            SaasBillingCycles.Yearly => 12,
            _ => 1
        };
}
