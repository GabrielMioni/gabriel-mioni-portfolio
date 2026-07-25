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
public sealed class DeleteProjectTests(SqlServerFixture database)
{
    private const string DeleteProjectMutation =
        """
        mutation DeleteProject($input: DeleteProjectInput!) {
          deleteProject(input: $input) {
            deletedProjectId
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendDeleteProjectAsync(
        HttpClient client,
        Guid projectId)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = DeleteProjectMutation,
                variables = new
                {
                    input = new
                    {
                        projectId
                    }
                }
            });
    }

    [Fact]
    public async Task DeleteProject_WhenProjectExists_ReturnsIdAndDeletesProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            title: "Project to delete",
            summary: "Created by an integration test.",
            body: null);

        var fullKey = $"projects/{project.Id}/full/delete-test.jpg";
        var thumbKey = $"projects/{project.Id}/thumb/delete-test.jpg";

        project.AddImage(ProjectImage.CreatePending(
            projectId: project.Id,
            clientId: "delete-test-image",
            altText: "Image belonging to the project being deleted.",
            fullKey: fullKey,
            thumbKey: thumbKey,
            contentType: "image/jpeg",
            sizeBytes: 1_024,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendDeleteProjectAsync(client, project.Id);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProject");

        Assert.Equal(
            project.Id,
            payload.GetProperty("deletedProjectId").GetGuid());
        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Projects.AnyAsync(p => p.Id == project.Id));

        Assert.Equal(
            new[] { fullKey, thumbKey }.Order(),
            factory.ObjectStorage.DeletedKeys.Order());
    }

    [Fact]
    public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNullAndNotFoundErrorCode()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendDeleteProjectAsync(client, missingProjectId);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("deletedProjectId").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.NotFound,
            message: $"Project '{missingProjectId}' was not found.");
    }
}
