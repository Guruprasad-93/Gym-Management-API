using Gym.Application.Validation;
using Xunit;

namespace Gym.API.IntegrationTests;

public class LoginIdentifierRulesTests
{
    [Theory]
    [InlineData("admin")]
    [InlineData("admin@fitzone-demo.com")]
    [InlineData("9876543210")]
    [InlineData("EMP001")]
    [InlineData("MEM000123")]
    [InlineData("guru.prasad")]
    public void Validate_AcceptsFlexibleFormats(string loginIdentifier)
    {
        var ex = Record.Exception(() => LoginIdentifierRules.Validate(loginIdentifier));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(" admin ", "admin")]
    [InlineData("admin@fitzone-demo.com ", "admin@fitzone-demo.com")]
    public void Normalize_TrimsWhitespace(string input, string expected)
    {
        Assert.Equal(expected, LoginIdentifierRules.Normalize(input));
    }

    [Fact]
    public void Validate_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => LoginIdentifierRules.Validate("   "));
    }

    [Fact]
    public void Validate_RejectsOverMaxLength()
    {
        Assert.Throws<ArgumentException>(() => LoginIdentifierRules.Validate(new string('a', 101)));
    }
}
