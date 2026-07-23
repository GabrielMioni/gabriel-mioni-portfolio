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
        using var response = await client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = """
                    mutation createProject($input: CreateProjectInput!) {
                      createProject(input: $input) {
                        project {
                            id
                            title
                            body
                            summary
                            status
                        }
                        userErrors {
                          code
                          message
                          field
                        }
                      }
                    }
                    """,
                variables = new
                {
                    input = new
                    {
                        title = projectTitle,
                        summary = projectSummary,
                        body = projectBody,
                        status = projectStatus
                    }
                }
            });

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
}