using Gym.Application.Constants;
using Gym.Application.Services;
using Xunit;

namespace Gym.API.IntegrationTests;

public class SaasBillingCycleTests
{
    private static readonly DateTime ExampleStart = new(2026, 7, 1);

    [Theory]
    [InlineData(SaasBillingCycles.Monthly, "2026-08-01")]
    [InlineData(SaasBillingCycles.Quarterly, "2026-10-01")]
    [InlineData(SaasBillingCycles.HalfYearly, "2027-01-01")]
    [InlineData(SaasBillingCycles.Yearly, "2027-07-01")]
    public void CalculatePeriodEnd_MatchesDocumentedExamples(string billingCycle, string expectedEndText)
    {
        var expectedEnd = DateTime.Parse(expectedEndText);
        var actualEnd = SaasBillingCycleHelper.CalculatePeriodEnd(ExampleStart, billingCycle);
        Assert.Equal(expectedEnd, actualEnd);
    }

    [Theory]
    [InlineData("Half-Yearly")]
    [InlineData("halfyearly")]
    [InlineData("HALF YEARLY")]
    public void Normalize_AcceptsHalfYearlyAliases(string input)
    {
        Assert.Equal(SaasBillingCycles.HalfYearly, SaasBillingCycleHelper.Normalize(input));
    }

    [Theory]
    [InlineData(SaasBillingCycles.Monthly, 1)]
    [InlineData(SaasBillingCycles.Quarterly, 3)]
    [InlineData(SaasBillingCycles.HalfYearly, 6)]
    [InlineData(SaasBillingCycles.Yearly, 12)]
    public void GetDurationMonths_ReturnsExpectedValues(string billingCycle, int expectedMonths)
    {
        Assert.Equal(expectedMonths, SaasBillingCycleHelper.GetDurationMonths(billingCycle));
    }

    [Fact]
    public void ResolvePrice_UsesCycleSpecificPlanPrice()
    {
        var plan = new Gym.Application.DTOs.Saas.SaasPlanDto
        {
            MonthlyPrice = 999,
            QuarterlyPrice = 2799,
            HalfYearlyPrice = 5399,
            YearlyPrice = 9990
        };

        Assert.Equal(999, SaasBillingCycleHelper.ResolvePrice(plan, SaasBillingCycles.Monthly));
        Assert.Equal(2799, SaasBillingCycleHelper.ResolvePrice(plan, SaasBillingCycles.Quarterly));
        Assert.Equal(5399, SaasBillingCycleHelper.ResolvePrice(plan, SaasBillingCycles.HalfYearly));
        Assert.Equal(9990, SaasBillingCycleHelper.ResolvePrice(plan, SaasBillingCycles.Yearly));
    }
}
