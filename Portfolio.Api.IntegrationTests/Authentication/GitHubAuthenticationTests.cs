using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.Authentication;

[Collection(IntegrationTestCollection.Name)]
public sealed class GitHubAuthenticationTests(SqlServerFixture database)
{
    [Fact]
    public async Task Login_WhenProxied_UsesForwardedOriginForCallback()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/auth/github/login");
        request.Headers.Add("X-Forwarded-Host", "localhost:3000");
        request.Headers.Add("X-Forwarded-Proto", "http");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var query = QueryHelpers.ParseQuery(response.Headers.Location.Query);
        Assert.Equal(
            "http://localhost:3000/api/auth/github/callback",
            query["redirect_uri"]);
    }
}
