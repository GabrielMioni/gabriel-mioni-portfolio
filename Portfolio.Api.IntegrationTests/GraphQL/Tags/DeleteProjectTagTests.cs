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
public sealed class DeleteProjectTagTests(SqlServerFixture database)
{
    private const string DeleteProjectTagMutation =
        """
        mutation DeleteProjectTag($input: DeleteProjectTagInput!) {
          deleteProjectTag(input: $input) {
            deletedTagId
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendDeleteProjectTagAsync(
        HttpClient client,
        Guid tagId)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = DeleteProjectTagMutation,
                variables = new
                {
                    input = new
                    {
                        id = tagId
                    }
                }
            });
    }

    [Fact]
    public async Task DeleteProjectTag_WhenTagExists_DeletesTagAndPreservesProjects()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var targetTag = ProjectTag.Create($"Target Tag {suffix}");
        var unrelatedTag = ProjectTag.Create($"Unrelated Tag {suffix}");

        var projectA = Project.Create($"Project A {suffix}", null, null);
        var projectB = Project.Create($"Project B {suffix}", null, null);

        projectA.AddTag(targetTag);
        projectA.AddTag(unrelatedTag);
        projectB.AddTag(targetTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectA, projectB);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendDeleteProjectTagAsync(
            client,
            tagId: targetTag.Id);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjectTag");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(
            targetTag.Id,
            payload.GetProperty("deletedTagId").GetGuid());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Tags
            .AnyAsync(tag => tag.Id == targetTag.Id));

        var persistedProjects = await verificationDb.Projects
            .Include(project => project.Tags)
            .AsNoTracking()
            .Where(project => project.Id == projectA.Id || project.Id == projectB.Id)
            .ToDictionaryAsync(project => project.Id);

        Assert.Equal(2, persistedProjects.Count);

        var persistedProjectATag = Assert.Single(
            persistedProjects[projectA.Id].Tags);

        Assert.Equal(unrelatedTag.Id, persistedProjectATag.Id);
        Assert.Empty(persistedProjects[projectB.Id].Tags);
    }

    [Fact]
    public async Task DeleteProjectTag_WhenTagDoesNotExist_ReturnsNotFoundAndChangesNothing()
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

        var missingTagId = Guid.NewGuid();

        // Act
        using var response = await SendDeleteProjectTagAsync(
            client,
            tagId: missingTagId);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjectTag");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("deletedTagId").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.NotFound,
            message: $"Project tag '{missingTagId}' was not found.",
            field: ["input", "id"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.True(await verificationDb.Tags
            .AnyAsync(tag => tag.Id == existingTag.Id));
    }
}
