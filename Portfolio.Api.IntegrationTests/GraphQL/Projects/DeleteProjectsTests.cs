using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class DeleteProjectsTests(SqlServerFixture database)
{
    private const string DeleteProjectsMutation =
        """
        mutation DeleteProjects($input: DeleteProjectsInput!) {
          deleteProjects(input: $input) {
            deletedProjectIds
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendDeleteProjectsAsync(
        HttpClient client,
        IReadOnlyList<Guid> projectIds)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = DeleteProjectsMutation,
                variables = new
                {
                    input = new
                    {
                        projectIds
                    }
                }
            });
    }

    [Fact]
    public async Task DeleteProjects_WithValidProjectIds_DeletesProjectsAndStoredImages()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var projectA = Project.Create($"Project A to delete {suffix}", null, null);
        var projectB = Project.Create($"Project B to delete {suffix}", null, null);
        var retainedProject = Project.Create($"Project to retain {suffix}", null, null);

        var projectAKeys = AddImage(projectA, "project-a");
        var projectBKeys = AddImage(projectB, "project-b");
        var retainedProjectKeys = AddImage(retainedProject, "retained-project");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectA, projectB, retainedProject);
            await db.SaveChangesAsync();
        }

        var requestedProjectIds = new[] { projectA.Id, projectB.Id };

        // Act
        using var response = await SendDeleteProjectsAsync(client, requestedProjectIds);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjects");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        var returnedProjectIds = payload
            .GetProperty("deletedProjectIds")
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

        Assert.False(await verificationDb.Projects.AnyAsync(
            project => requestedProjectIds.Contains(project.Id)));

        Assert.True(await verificationDb.Projects.AnyAsync(
            project => project.Id == retainedProject.Id));

        var expectedDeletedKeys = new[]
        {
            projectAKeys.FullKey,
            projectAKeys.ThumbKey,
            projectBKeys.FullKey,
            projectBKeys.ThumbKey
        }.Order();

        Assert.Equal(
            expectedDeletedKeys,
            factory.ObjectStorage.DeletedKeys.Order());

        Assert.DoesNotContain(
            retainedProjectKeys.FullKey,
            factory.ObjectStorage.DeletedKeys);
        Assert.DoesNotContain(
            retainedProjectKeys.ThumbKey,
            factory.ObjectStorage.DeletedKeys);
    }

    [Fact]
    public async Task DeleteProjects_WithMissingProjectId_ReturnsInvalidReferenceAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project to retain {suffix}", null, null);
        AddImage(project, "retained-project");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendDeleteProjectsAsync(
            client,
            [project.Id, missingProjectId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjects");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("deletedProjectIds").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project '{missingProjectId}' was not found.",
            field: ["input", "projectIds", "1"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(item => item.Images)
            .AsNoTracking()
            .SingleAsync(item => item.Id == project.Id);

        Assert.Single(persistedProject.Images);
        Assert.Empty(factory.ObjectStorage.DeletedKeys);
    }

    private static (string FullKey, string ThumbKey) AddImage(
        Project project,
        string clientId)
    {
        var fullKey = $"projects/{project.Id}/full/{clientId}.jpg";
        var thumbKey = $"projects/{project.Id}/thumb/{clientId}.jpg";

        project.AddImage(ProjectImage.CreatePending(
            id: Guid.NewGuid(),
            projectId: project.Id,
            clientId: clientId,
            altText: null,
            fullKey: fullKey,
            thumbKey: thumbKey,
            contentType: "image/jpeg",
            sizeBytes: 1_024,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        return (fullKey, thumbKey);
    }
}
