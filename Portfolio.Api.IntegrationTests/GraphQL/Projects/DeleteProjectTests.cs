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

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendDeleteProjectAsync(client, project.Id);

        // Assert: public GraphQL contract
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("deleteProject");

        Assert.Equal(
            project.Id,
            payload.GetProperty("deletedProjectId").GetGuid());
        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Projects.AnyAsync(p => p.Id == project.Id));
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
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("deleteProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("deletedProjectId").ValueKind);

        var userError = Assert.Single(
            payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(
            "NOT_FOUND",
            userError.GetProperty("code").GetString());
    }
}
