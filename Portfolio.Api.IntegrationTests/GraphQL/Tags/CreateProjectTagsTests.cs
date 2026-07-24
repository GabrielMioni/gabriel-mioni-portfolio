using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Tags;

[Collection(IntegrationTestCollection.Name)]
public sealed class CreateProjectTagsTests(SqlServerFixture database)
{
    private const string CreateProjectTagsMutation =
        """
        mutation CreateProjectTags($input: CreateProjectTagsInput!) {
          createProjectTags(input: $input) {
            tags {
              id
              name
              value
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private sealed record ReturnedTag(
        Guid Id,
        string Name,
        string Value);

    private static Task<HttpResponseMessage> SendCreateProjectTagsAsync(
        HttpClient client,
        IReadOnlyList<string> names)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = CreateProjectTagsMutation,
                variables = new
                {
                    input = new
                    {
                        names
                    }
                }
            });
    }

    [Fact]
    public async Task CreateProjectTags_WithValidNames_ReturnsAndPersistsNormalizedTags()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var names = new[]
        {
            $"  First Tag {suffix}  ",
            $"Second Tag {suffix}"
        };

        // Act
        using var response = await SendCreateProjectTagsAsync(client, names);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("createProjectTags");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        var returnedTags = payload
            .GetProperty("tags")
            .EnumerateArray()
            .Select(tag => new ReturnedTag(
                Id: tag.GetProperty("id").GetGuid(),
                Name: tag.GetProperty("name").GetString()!,
                Value: tag.GetProperty("value").GetString()!))
            .OrderBy(tag => tag.Name)
            .ToArray();

        Assert.Collection(
            returnedTags,
            tag =>
            {
                Assert.NotEqual(Guid.Empty, tag.Id);
                Assert.Equal($"First Tag {suffix}", tag.Name);
                Assert.Equal($"first-tag-{suffix}", tag.Value);
            },
            tag =>
            {
                Assert.NotEqual(Guid.Empty, tag.Id);
                Assert.Equal($"Second Tag {suffix}", tag.Name);
                Assert.Equal($"second-tag-{suffix}", tag.Value);
            });

        Assert.Equal(
            returnedTags.Length,
            returnedTags.Select(tag => tag.Id).Distinct().Count());

        // Assert: persisted state
        var returnedTagIds = returnedTags
            .Select(tag => tag.Id)
            .ToArray();

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedTags = await verificationDb.Tags
            .AsNoTracking()
            .Where(tag => returnedTagIds.Contains(tag.Id))
            .OrderBy(tag => tag.Name)
            .Select(tag => new ReturnedTag(tag.Id, tag.Name, tag.Value))
            .ToArrayAsync();

        Assert.Equal(returnedTags, persistedTags);
    }

    [Fact]
    public async Task CreateProjectTags_WithExistingValue_ReturnsConflictAndCreatesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var existingTag = ProjectTag.Create($"Existing Tag {suffix}");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tags.Add(existingTag);
            await db.SaveChangesAsync();
        }

        var newTagName = $"New Tag {suffix}";
        var conflictingName = $"  {existingTag.Name}  ";

        // Act
        using var response = await SendCreateProjectTagsAsync(
            client,
            names: [newTagName, conflictingName]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("createProjectTags");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("tags").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Conflict,
            message: $"A tag with the name '{existingTag.Name}' already exists.",
            field: ["input", "names", "1"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Tags
            .AnyAsync(tag => tag.Value == $"new-tag-{suffix}"));

        Assert.True(await verificationDb.Tags
            .AnyAsync(tag => tag.Id == existingTag.Id));
    }
}
