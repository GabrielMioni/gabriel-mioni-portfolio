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
public sealed class ProjectLifecycleTests(SqlServerFixture database)
{
    private const string PublishProjectMutation =
        """
        mutation PublishProject($input: PublishProjectInput!) {
          publishProject(input: $input) {
            project {
              id
              status
              publishedAt
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private const string ArchiveProjectMutation =
        """
        mutation ArchiveProject($input: ArchiveProjectInput!) {
          archiveProject(input: $input) {
            project {
              id
              status
              publishedAt
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    public static TheoryData<string, string> MissingProjectMutations =>
        new()
        {
            { PublishProjectMutation, "publishProject" },
            { ArchiveProjectMutation, "archiveProject" }
        };

    private static Task<HttpResponseMessage> SendLifecycleMutationAsync(
        HttpClient client,
        string mutation,
        Guid projectId)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = mutation,
                variables = new
                {
                    input = new
                    {
                        id = projectId
                    }
                }
            });
    }

    private static async Task<JsonElement> ReadPayloadAsync(
        HttpResponseMessage response,
        string payloadName)
    {
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("errors", out _), root.ToString());

        return root
            .GetProperty("data")
            .GetProperty(payloadName)
            .Clone();
    }

    [Fact]
    public async Task PublishProject_WhenProjectExists_ReturnsAndPersistsPublishedProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            title: $"Project to publish {Guid.NewGuid()}",
            summary: null,
            body: null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendLifecycleMutationAsync(
            client,
            PublishProjectMutation,
            project.Id);

        // Assert: public GraphQL contract
        var payload = await ReadPayloadAsync(response, "publishProject");
        var payloadProject = payload.GetProperty("project");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());
        Assert.Equal("PUBLISHED", payloadProject.GetProperty("status").GetString());
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

        Assert.Equal(ProjectStatus.Published, persistedProject.Status);
        Assert.NotNull(persistedProject.PublishedAt);
    }

    [Fact]
    public async Task ArchiveProject_WhenProjectExists_PreservesPublicationAndPersistsArchivedStatus()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            title: $"Project to archive {Guid.NewGuid()}",
            summary: null,
            body: null,
            status: ProjectStatus.Published);
        var originalPublishedAt = project.PublishedAt;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendLifecycleMutationAsync(
            client,
            ArchiveProjectMutation,
            project.Id);

        // Assert: public GraphQL contract
        var payload = await ReadPayloadAsync(response, "archiveProject");
        var payloadProject = payload.GetProperty("project");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());
        Assert.Equal("ARCHIVED", payloadProject.GetProperty("status").GetString());
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

        Assert.Equal(ProjectStatus.Archived, persistedProject.Status);
        Assert.Equal(originalPublishedAt, persistedProject.PublishedAt);
    }

    [Theory]
    [MemberData(nameof(MissingProjectMutations))]
    public async Task ProjectLifecycle_WhenProjectDoesNotExist_ReturnsNotFound(
        string mutation,
        string payloadName)
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendLifecycleMutationAsync(
            client,
            mutation,
            missingProjectId);

        // Assert
        var payload = await ReadPayloadAsync(response, payloadName);

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        var userError = Assert.Single(
            payload.GetProperty("userErrors").EnumerateArray());

        Assert.Equal("NOT_FOUND", userError.GetProperty("code").GetString());
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
