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
public sealed class RenameProjectTagTests(SqlServerFixture database)
{
    private const string RenameProjectTagMutation =
        """
        mutation RenameProjectTag($input: RenameProjectTagInput!) {
          renameProjectTag(input: $input) {
            tag {
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

    private static Task<HttpResponseMessage> SendRenameProjectTagAsync(
        HttpClient client,
        Guid tagId,
        string name)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = RenameProjectTagMutation,
                variables = new
                {
                    input = new
                    {
                        id = tagId,
                        name
                    }
                }
            });
    }

    [Fact]
    public async Task RenameProjectTag_WithValidName_ReturnsAndPersistsNormalizedTag()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var tag = ProjectTag.Create($"Original Tag {suffix}");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
        }

        var newName = $"  Renamed Tag {suffix}  ";
        var expectedName = $"Renamed Tag {suffix}";
        var expectedValue = $"renamed-tag-{suffix}";

        // Act
        using var response = await SendRenameProjectTagAsync(
            client,
            tagId: tag.Id,
            name: newName);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("renameProjectTag");
        var returnedTag = payload.GetProperty("tag");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(tag.Id, returnedTag.GetProperty("id").GetGuid());
        Assert.Equal(expectedName, returnedTag.GetProperty("name").GetString());
        Assert.Equal(expectedValue, returnedTag.GetProperty("value").GetString());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedTag = await verificationDb.Tags
            .AsNoTracking()
            .SingleAsync(item => item.Id == tag.Id);

        Assert.Equal(expectedName, persistedTag.Name);
        Assert.Equal(expectedValue, persistedTag.Value);
    }

    [Fact]
    public async Task RenameProjectTag_WithExistingValue_ReturnsConflictAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var tagToRename = ProjectTag.Create($"Original Tag {suffix}");
        var conflictingTag = ProjectTag.Create($"Existing Tag {suffix}");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tags.AddRange(tagToRename, conflictingTag);
            await db.SaveChangesAsync();
        }

        var originalName = tagToRename.Name;
        var originalValue = tagToRename.Value;
        var conflictingName = $"  {conflictingTag.Name}  ";

        // Act
        using var response = await SendRenameProjectTagAsync(
            client,
            tagId: tagToRename.Id,
            name: conflictingName);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("renameProjectTag");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("tag").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Conflict,
            message: $"A tag with the name '{conflictingTag.Name}' already exists.",
            field: ["input", "name"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedTags = await verificationDb.Tags
            .AsNoTracking()
            .Where(tag => tag.Id == tagToRename.Id || tag.Id == conflictingTag.Id)
            .ToDictionaryAsync(tag => tag.Id);

        Assert.Equal(originalName, persistedTags[tagToRename.Id].Name);
        Assert.Equal(originalValue, persistedTags[tagToRename.Id].Value);
        Assert.Equal(conflictingTag.Name, persistedTags[conflictingTag.Id].Name);
        Assert.Equal(conflictingTag.Value, persistedTags[conflictingTag.Id].Value);
    }
}
