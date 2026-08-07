using System.Net;
using System.Net.Http.Json;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Authentication;

[Collection(IntegrationTestCollection.Name)]
public sealed class GraphQlAuthenticationTests(SqlServerFixture database)
{
    private const string TypenameQuery =
        """
        query EndpointAvailability {
          __typename
        }
        """;

    private static Task<HttpResponseMessage> SendQueryAsync(
        HttpClient client,
        string path)
    {
        return client.PostAsJsonAsync(
            path,
            new
            {
                query = TypenameQuery
            });
    }

    [Fact]
    public async Task AdminGraphQl_WhenUnauthenticated_ReturnsUnauthorized()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await SendQueryAsync(client, "/graphql/admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminGraphQl_WhenAuthenticatedAsAdmin_ReturnsSuccessfulResponse()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        using var response = await SendQueryAsync(client, "/graphql/admin");

        var data = await response.ReadGraphQlDataAsync();

        Assert.Equal(
            "Query",
            data.GetProperty("__typename").GetString());
    }

    [Fact]
    public async Task AdminGraphQl_WhenAuthenticatedWithoutAdminRole_ReturnsForbidden()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient(isAdmin: false);

        using var response = await SendQueryAsync(client, "/graphql/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PublicGraphQl_WhenUnauthenticated_ReturnsSuccessfulResponse()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        using var response = await SendQueryAsync(client, "/graphql");

        var data = await response.ReadGraphQlDataAsync();

        Assert.Equal(
            "Query",
            data.GetProperty("__typename").GetString());
    }
}
