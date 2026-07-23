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
public sealed class CreateProjectTests(SqlServerFixture database)
{
    private const string CreateProjectMutation =
        """
        mutation CreateProject($input: CreateProjectInput!) {
          createProject(input: $input) {
            project {
              id
              title
              summary
              body
              status
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private static Task<HttpResponseMessage> SendCreateProjectAsync(
        HttpClient client,
        string title,
        string? summary,
        string? body,
        string status = "DRAFT")
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = CreateProjectMutation,
                variables = new
                {
                    input = new
                    {
                        title,
                        summary,
                        body,
                        status
                    }
                }
            });
    }

    [Fact]
    public async Task CreateProject_WithValidInput_ReturnsAndPersistsProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var projectTitle = "New Project";
        var projectSummary = "This is a new project created by an integration test.";
        var projectBody = "This is the body of the new project.";
        const string projectStatus = "DRAFT";

        // Act
        using var response = await SendCreateProjectAsync(
            client,
            projectTitle,
            projectSummary,
            projectBody,
            projectStatus);

        // Assert: public GraphQL contract
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("createProject");

        var payloadProject = payload.GetProperty("project");
        var projectId = payloadProject.GetProperty("id").GetGuid();

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(projectTitle, payloadProject.GetProperty("title").GetString());
        Assert.Equal(projectSummary, payloadProject.GetProperty("summary").GetString());
        Assert.Equal(projectBody, payloadProject.GetProperty("body").GetString());
        Assert.Equal(projectStatus, payloadProject.GetProperty("status").GetString());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .SingleAsync(p => p.Id == projectId);

        Assert.Equal(projectTitle, persistedProject.Title);
        Assert.Equal(projectSummary, persistedProject.Summary);
        Assert.Equal(projectBody, persistedProject.Body);
        Assert.Equal(ProjectStatus.Draft, persistedProject.Status);
    }

    [Fact]
    public async Task CreateProject_WithWhitespaceTitle_ReturnsValidationErrorAndDoesNotPersistProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var invalidTitle = "   ";
        var projectSummary = $"invalid-project-{Guid.NewGuid()}";
        var projectBody = "This is the body of the new project.";
        const string projectStatus = "DRAFT";

        // Act
        using var response = await SendCreateProjectAsync(
            client,
            invalidTitle,
            projectSummary,
            projectBody,
            projectStatus);

        // Assert: public GraphQL contract
        response.EnsureSuccessStatusCode();

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Projects
            .AnyAsync(p => p.Summary == projectSummary));

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;
        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        var payload = root
            .GetProperty("data")
            .GetProperty("createProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        var userError = Assert.Single(
            payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal(
            "VALIDATION",
            userError.GetProperty("code").GetString());

        Assert.Equal(
            "Title is required.",
            userError.GetProperty("message").GetString());

        var field = userError
            .GetProperty("field")
            .EnumerateArray();

        Assert.Collection(
            field,
            item => Assert.Equal("input", item.GetString()),
            item => Assert.Equal("title", item.GetString()));
    }
}
