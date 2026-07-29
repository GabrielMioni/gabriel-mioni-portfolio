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
public sealed class UpdateProjectTagsTests(SqlServerFixture database)
{
    private const string UpdateProjectTagsMutation =
        """
        mutation UpdateProjectTags($input: UpdateProjectTagsInput!) {
          updateProjectTags(input: $input) {
            project {
              id
              tags {
                id
                name
                value
              }
            }
            userErrors {
              code
              message
              field
            }
          }
        }
        """;

    private sealed record ReturnedTag(
        Guid Id,
        string Name,
        string Value);

    private static Task<HttpResponseMessage> SendUpdateProjectTagsAsync(
        HttpClient client,
        Guid projectId,
        IReadOnlyList<Guid> tagIds)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = UpdateProjectTagsMutation,
                variables = new
                {
                    input = new
                    {
                        projectId,
                        tagIds
                    }
                }
            });
    }

    [Fact]
    public async Task UpdateProjectTags_WithValidTagIds_ReplacesAndPersistsDesiredTagSet()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create(
            title: $"Project with replaceable tags {suffix}",
            summary: null,
            body: null);

        var tagA = ProjectTag.Create($"Tag A {suffix}");
        var tagB = ProjectTag.Create($"Tag B {suffix}");
        var tagC = ProjectTag.Create($"Tag C {suffix}");

        project.AddTag(tagA);
        project.AddTag(tagB);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            db.Tags.Add(tagC);
            await db.SaveChangesAsync();
        }

        var desiredTagIds = new[] { tagB.Id, tagC.Id };

        // Act
        using var response = await SendUpdateProjectTagsAsync(
            client,
            projectId: project.Id,
            tagIds: desiredTagIds);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("updateProjectTags");
        var payloadProject = payload.GetProperty("project");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());

        var returnedTags = payloadProject
            .GetProperty("tags")
            .EnumerateArray()
            .Select(tag => new ReturnedTag(
                Id: tag.GetProperty("id").GetGuid(),
                Name: tag.GetProperty("name").GetString()!,
                Value: tag.GetProperty("value").GetString()!))
            .OrderBy(tag => tag.Id)
            .ToArray();

        var expectedTags = new[]
        {
            new ReturnedTag(tagB.Id, tagB.Name, tagB.Value),
            new ReturnedTag(tagC.Id, tagC.Name, tagC.Value)
        }
        .OrderBy(tag => tag.Id)
        .ToArray();

        Assert.Equal(expectedTags, returnedTags);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(p => p.Tags)
            .AsNoTracking()
            .SingleAsync(p => p.Id == project.Id);

        var persistedTagIds = persistedProject.Tags
            .Select(tag => tag.Id)
            .OrderBy(id => id)
            .ToArray();

        Assert.Equal(
            desiredTagIds.OrderBy(id => id),
            persistedTagIds);
    }

    [Fact]
    public async Task UpdateProjectTags_AboveProjectTagLimit_ReturnsValidationAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create(
            title: $"Project above tag limit {suffix}",
            summary: null,
            body: null);

        var tags = Enumerable
            .Range(0, Project.MaxTagCount + 1)
            .Select(index => ProjectTag.Create($"Limit tag {index} {suffix}"))
            .ToArray();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            db.Tags.AddRange(tags);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendUpdateProjectTagsAsync(
            client,
            projectId: project.Id,
            tagIds: tags.Select(tag => tag.Id).ToArray());

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("updateProjectTags");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Validation,
            message: $"A project cannot have more than {Project.MaxTagCount} tags.",
            field: ["input", "tagIds"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(candidate => candidate.Tags)
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == project.Id);

        Assert.Empty(persistedProject.Tags);
    }

    [Fact]
    public async Task UpdateProjectTags_WithMissingTagId_ReturnsInvalidReferenceAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create(
            title: $"Project with unchanged tags {suffix}",
            summary: null,
            body: null);

        var originalTag = ProjectTag.Create($"Original tag {suffix}");
        var availableTag = ProjectTag.Create($"Available tag {suffix}");

        project.AddTag(originalTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            db.Tags.Add(availableTag);
            await db.SaveChangesAsync();
        }

        var missingTagId = Guid.NewGuid();

        // Act
        using var response = await SendUpdateProjectTagsAsync(
            client,
            projectId: project.Id,
            tagIds: [availableTag.Id, missingTagId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("updateProjectTags");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project tag '{missingTagId}' was not found.",
            field: ["input", "tagIds", "1"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(p => p.Tags)
            .AsNoTracking()
            .SingleAsync(p => p.Id == project.Id);

        var persistedTag = Assert.Single(persistedProject.Tags);
        Assert.Equal(originalTag.Id, persistedTag.Id);
        Assert.DoesNotContain(
            persistedProject.Tags,
            tag => tag.Id == availableTag.Id);
    }
}
