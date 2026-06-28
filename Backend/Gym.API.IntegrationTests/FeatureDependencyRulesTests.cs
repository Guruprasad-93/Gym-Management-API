using Gym.Application.Authorization;
using Xunit;

namespace Gym.API.IntegrationTests;

public class FeatureDependencyRulesTests
{
    [Fact]
    public void Validate_WebsiteBuilderWithoutPublicWebsite_Fails()
    {
        var result = FeatureDependencyRules.Validate(["WEBSITE_BUILDER"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Violations, v => v.RequiresFeatureCode == "PUBLIC_WEBSITE");
    }

    [Fact]
    public void Validate_WebsiteBuilderWithPublicWebsite_Passes()
    {
        var result = FeatureDependencyRules.Validate(["WEBSITE_BUILDER", "PUBLIC_WEBSITE"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AiInsightsWithoutReports_Fails()
    {
        var result = FeatureDependencyRules.Validate(["AI_INSIGHTS"]);
        Assert.False(result.IsValid);
        Assert.Contains(result.Violations, v => v.RequiresFeatureCode == "REPORTS");
    }

    [Fact]
    public void Validate_MultiBranchWithoutMembersOrTrainers_Fails()
    {
        var result = FeatureDependencyRules.Validate(["MULTI_BRANCH"]);
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Violations.Count);
    }

    [Fact]
    public void Validate_MultiBranchWithMembersAndTrainers_Passes()
    {
        var result = FeatureDependencyRules.Validate(["MULTI_BRANCH", "MEMBERS", "TRAINERS"]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_CoreFeaturesOnly_Passes()
    {
        var result = FeatureDependencyRules.Validate(["DASHBOARD", "MEMBERS", "ATTENDANCE"]);
        Assert.True(result.IsValid);
    }
}
