using Portfolio.Api.Authentication;
using Xunit;

namespace Portfolio.Api.IntegrationTests.Authentication;

public sealed class GitHubAuthenticationOptionsTests
{
    private readonly GitHubAuthenticationOptions _options = new()
    {
        AllowedUserId = "12345678"
    };

    [Fact]
    public void IsAllowedUser_WhenUserIdMatches_ReturnsTrue()
    {
        Assert.True(_options.IsAllowedUser("12345678"));
    }

    [Theory]
    [InlineData("87654321")]
    [InlineData("")]
    [InlineData(null)]
    public void IsAllowedUser_WhenUserIdDoesNotMatch_ReturnsFalse(string? userId)
    {
        Assert.False(_options.IsAllowedUser(userId));
    }
}
