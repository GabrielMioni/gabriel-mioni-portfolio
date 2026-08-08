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
        var payload = await response.ReadGraphQlPayloadAsync("createProject");

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
        var payload = await response.ReadGraphQlPayloadAsync("createProject");

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Projects
            .AnyAsync(p => p.Summary == projectSummary));

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Validation,
            message: "Title is required.",
            field: ["input", "title"]);
    }

    [Fact]
    public async Task CreateProject_AboveTextLimits_ReturnsValidationErrorsAndDoesNotPersistProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var invalidTitle = new string('t', Project.MaxTitleLength + 1);
        var invalidSummary = new string('s', Project.MaxSummaryLength + 1);
        var invalidBody = new string('b', Project.MaxBodyLength + 1);

        // Act
        using var response = await SendCreateProjectAsync(
            client,
            invalidTitle,
            invalidSummary,
            invalidBody);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("createProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        var actualErrors = payload
            .GetProperty("userErrors")
            .EnumerateArray()
            .Select(error => (
                Message: error.GetProperty("message").GetString()!,
                Field: string.Join(
                    ".",
                    error.GetProperty("field")
                        .EnumerateArray()
                        .Select(item => item.GetString()!))))
            .ToArray();

        Assert.Equal(
            [
                (
                    Message: $"Title cannot exceed {Project.MaxTitleLength} characters.",
                    Field: "input.title"),
                (
                    Message: $"Summary cannot exceed {Project.MaxSummaryLength} characters.",
                    Field: "input.summary"),
                (
                    Message: $"Body cannot exceed {Project.MaxBodyLength} characters.",
                    Field: "input.body")
            ],
            actualErrors);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.Projects
            .AnyAsync(project => project.Title == invalidTitle));
    }
}
