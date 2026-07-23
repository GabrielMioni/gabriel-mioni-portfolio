using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class EditProjectTests(SqlServerFixture database)
{
    private const string EditProjectMutation =
        """
        mutation EditProject($input: EditProjectInput!) {
          editProject(input: $input) {
            project {
              id
              title
              summary
              body
              status
              publishedAt
              createdAt
              updatedAt
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendEditProjectAsync(
        HttpClient client,
        Guid projectId,
        string? title = null,
        string? summary = null,
        string? body = null,
        string? status = null)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = EditProjectMutation,
                variables = new
                {
                    input = new
                    {
                        id = projectId,
                        title,
                        summary,
                        body,
                        status
                    }
                }
            });
    }

    [Fact]
    public async Task EditProject_WithValidScalarChanges_ReturnsAndPersistsUpdatedProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var originalGuid = Guid.NewGuid();
        var originalTitle = $"original-project-title-{originalGuid}";
        var originalSummary = $"original-project-summary-{originalGuid}";
        var originalBody = $"original-project-body-{originalGuid}";

        var project = Project.Create(
            title: originalTitle,
            summary: originalSummary,
            body: originalBody);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var updatedGuid = Guid.NewGuid();
        var newTitle = $"updated-project-title-{updatedGuid}";
        var newSummary = $"updated-project-summary-{updatedGuid}";
        var newBody = $"updated-project-body-{updatedGuid}";
        var newStatus = "PUBLISHED";

        // Act
        using var response = await SendEditProjectAsync(
            client,
            projectId: project.Id,
            title: newTitle,
            summary: newSummary,
            body: newBody,
            status: newStatus);

        // Assert: public GraphQL contract
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("editProject");

        var payloadProject = payload.GetProperty("project");
        var returnedProjectId = payloadProject.GetProperty("id").GetGuid();

        Assert.Equal(project.Id, returnedProjectId);

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(newTitle, payloadProject.GetProperty("title").GetString());
        Assert.Equal(newSummary, payloadProject.GetProperty("summary").GetString());
        Assert.Equal(newBody, payloadProject.GetProperty("body").GetString());
        Assert.Equal(newStatus, payloadProject.GetProperty("status").GetString());

        Assert.NotEqual(
            JsonValueKind.Null,
            payloadProject.GetProperty("publishedAt").ValueKind);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .AsNoTracking()
            .SingleAsync(p => p.Id == project.Id);

        Assert.NotNull(persistedProject.PublishedAt);

        Assert.Equal(newTitle, persistedProject.Title);
        Assert.Equal(newSummary, persistedProject.Summary);
        Assert.Equal(newBody, persistedProject.Body);
        Assert.Equal(ProjectStatus.Published, persistedProject.Status);
    }

    [Fact]
    public async Task EditProject_WhenProjectDoesNotExist_ReturnsNullAndNotFoundErrorCode()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();
        var newTitle = $"updated-project-title-{missingProjectId}";
        var newSummary = $"updated-project-summary-{missingProjectId}";
        var newBody = $"updated-project-body-{missingProjectId}";

        // Act
        using var response = await SendEditProjectAsync(
            client,
            projectId: missingProjectId,
            title: newTitle,
            summary: newSummary,
            body: newBody);

        // Assert: public GraphQL contract
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("editProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        var userError = Assert.Single(
            payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(
            "NOT_FOUND",
            userError.GetProperty("code").GetString());

        Assert.Equal(
            $"Project '{missingProjectId}' was not found.",
            userError.GetProperty("message").GetString());

        var field = userError
            .GetProperty("field")
            .EnumerateArray();

        Assert.Collection(
            field,
            item => Assert.Equal("input", item.GetString()),
            item => Assert.Equal("id", item.GetString()));
    }
}
