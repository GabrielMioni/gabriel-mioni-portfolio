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
public sealed class RemoveTagFromProjectsTests(SqlServerFixture database)
{
    private const string RemoveTagFromProjectsMutation =
        """
        mutation RemoveTagFromProjects($input: RemoveTagFromProjectsInput!) {
          removeTagFromProjects(input: $input) {
            projectIds
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendRemoveTagFromProjectsAsync(
        HttpClient client,
        Guid tagId,
        IReadOnlyList<Guid> projectIds)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = RemoveTagFromProjectsMutation,
                variables = new
                {
                    input = new
                    {
                        tagId,
                        projectIds
                    }
                }
            });
    }

    [Fact]
    public async Task RemoveTagFromProjects_WithValidProjectIds_RemovesOnlyRequestedRelationships()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var targetTag = ProjectTag.Create($"Target Tag {suffix}");
        var unrelatedTag = ProjectTag.Create($"Unrelated Tag {suffix}");

        var projectA = Project.Create($"Project A {suffix}", null, null);
        var projectB = Project.Create($"Project B {suffix}", null, null);
        var projectC = Project.Create($"Project C {suffix}", null, null);

        projectA.AddTag(targetTag);
        projectA.AddTag(unrelatedTag);
        projectB.AddTag(targetTag);
        projectC.AddTag(targetTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectA, projectB, projectC);
            await db.SaveChangesAsync();
        }

        var requestedProjectIds = new[] { projectA.Id, projectB.Id };

        // Act
        using var response = await SendRemoveTagFromProjectsAsync(
            client,
            tagId: targetTag.Id,
            projectIds: requestedProjectIds);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("removeTagFromProjects");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        var returnedProjectIds = payload
            .GetProperty("projectIds")
            .EnumerateArray()
            .Select(projectId => projectId.GetGuid())
            .OrderBy(projectId => projectId)
            .ToArray();

        Assert.Equal(
            requestedProjectIds.OrderBy(projectId => projectId),
            returnedProjectIds);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProjects = await verificationDb.Projects
            .Include(project => project.Tags)
            .AsNoTracking()
            .Where(project =>
                project.Id == projectA.Id ||
                project.Id == projectB.Id ||
                project.Id == projectC.Id)
            .ToDictionaryAsync(project => project.Id);

        var persistedProjectATag = Assert.Single(
            persistedProjects[projectA.Id].Tags);

        Assert.Equal(unrelatedTag.Id, persistedProjectATag.Id);
        Assert.Empty(persistedProjects[projectB.Id].Tags);

        var persistedProjectCTag = Assert.Single(
            persistedProjects[projectC.Id].Tags);

        Assert.Equal(targetTag.Id, persistedProjectCTag.Id);
        Assert.True(await verificationDb.Tags
            .AnyAsync(tag => tag.Id == targetTag.Id));
    }

    [Fact]
    public async Task RemoveTagFromProjects_WithMissingProjectId_ReturnsInvalidReferenceAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var targetTag = ProjectTag.Create($"Target Tag {suffix}");
        var project = Project.Create($"Unchanged Project {suffix}", null, null);

        project.AddTag(targetTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendRemoveTagFromProjectsAsync(
            client,
            tagId: targetTag.Id,
            projectIds: [project.Id, missingProjectId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("removeTagFromProjects");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("projectIds").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project '{missingProjectId}' was not found.",
            field: ["input", "projectIds", "1"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(item => item.Tags)
            .AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);

        var persistedTag = Assert.Single(persistedProject.Tags);
        Assert.Equal(targetTag.Id, persistedTag.Id);
    }
}
