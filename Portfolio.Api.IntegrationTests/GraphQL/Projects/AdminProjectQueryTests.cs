using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class AdminProjectQueryTests(SqlServerFixture database)
{
    private const string ProjectByIdQuery =
        """
        query ProjectById($id: UUID!) {
          projectById(id: $id) {
            id
            title
            summary
            body
            status
            images {
              id
              altText
              sortOrder
            }
            links {
              id
              url
              linkText
              linkType
              sortOrder
            }
            tags {
              id
              name
              value
            }
          }
        }
        """;

    private const string ProjectsQuery =
        """
        query Projects(
          $skip: Int
          $take: Int
          $includeUnpublished: Boolean!
          $where: ProjectFilterInput
          $order: [ProjectSortInput!]
        ) {
          projects(
            skip: $skip
            take: $take
            includeUnpublished: $includeUnpublished
            where: $where
            order: $order
          ) {
            items {
              id
              title
              status
            }
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
            totalCount
          }
        }
        """;

    private static Task<HttpResponseMessage> SendProjectByIdAsync(
        HttpClient client,
        Guid projectId)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = ProjectByIdQuery,
                variables = new { id = projectId }
            });
    }

    private static Task<HttpResponseMessage> SendProjectsAsync(
        HttpClient client,
        bool includeUnpublished,
        int? skip = null,
        int? take = null,
        object? where = null,
        object[]? order = null)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = ProjectsQuery,
                variables = new
                {
                    skip,
                    take,
                    includeUnpublished,
                    where,
                    order
                }
            });
    }

    [Fact]
    public async Task ProjectById_WhenProjectExists_ReturnsCompleteProject()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create(
            title: $"Admin detail project {suffix}",
            summary: "Admin detail summary",
            body: "Admin detail body");

        var image = ProjectImage.CreatePending(
            id: Guid.NewGuid(),
            project.Id,
            clientId: $"admin-detail-{suffix}",
            altText: "Admin detail image",
            fullKey: $"projects/{project.Id}/full-{suffix}.webp",
            thumbKey: $"projects/{project.Id}/thumb-{suffix}.webp",
            contentType: "image/webp",
            sizeBytes: 2_048,
            width: 1_600,
            height: 900,
            sortOrder: 0);
        image.MarkUploaded();
        project.AddImage(image);

        var link = ProjectLink.Create(
            project.Id,
            "https://example.com/admin-detail",
            "Admin detail link",
            ProjectLinkType.External,
            sortOrder: 0);
        project.AddLink(link);

        var tag = ProjectTag.Create($"Admin detail tag {suffix}");
        project.AddTag(tag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendProjectByIdAsync(client, project.Id);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var returnedProject = data.GetProperty("projectById");

        Assert.Equal(project.Id, returnedProject.GetProperty("id").GetGuid());
        Assert.Equal(project.Title, returnedProject.GetProperty("title").GetString());
        Assert.Equal(project.Summary, returnedProject.GetProperty("summary").GetString());
        Assert.Equal(project.Body, returnedProject.GetProperty("body").GetString());
        Assert.Equal("DRAFT", returnedProject.GetProperty("status").GetString());

        var returnedImage = Assert.Single(
            returnedProject.GetProperty("images").EnumerateArray());
        Assert.Equal(image.Id, returnedImage.GetProperty("id").GetGuid());
        Assert.Equal(image.AltText, returnedImage.GetProperty("altText").GetString());

        var returnedLink = Assert.Single(
            returnedProject.GetProperty("links").EnumerateArray());
        Assert.Equal(link.Id, returnedLink.GetProperty("id").GetGuid());
        Assert.Equal(link.Url, returnedLink.GetProperty("url").GetString());

        var returnedTag = Assert.Single(
            returnedProject.GetProperty("tags").EnumerateArray());
        Assert.Equal(tag.Id, returnedTag.GetProperty("id").GetGuid());
        Assert.Equal(tag.Value, returnedTag.GetProperty("value").GetString());
    }

    [Fact]
    public async Task ProjectById_WhenProjectDoesNotExist_ReturnsNull()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Act
        using var response = await SendProjectByIdAsync(client, Guid.NewGuid());

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        Assert.Equal(JsonValueKind.Null, data.GetProperty("projectById").ValueKind);
    }

    [Fact]
    public async Task Projects_IncludeUnpublishedControlsWhichStatusesAreReturned()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var draft = Project.Create($"Status draft {suffix}", null, null);
        var published = Project.Create(
            $"Status published {suffix}",
            null,
            null,
            ProjectStatus.Published);
        var archived = Project.Create(
            $"Status archived {suffix}",
            null,
            null,
            ProjectStatus.Archived);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(draft, published, archived);
            await db.SaveChangesAsync();
        }

        var where = new { title = new { contains = suffix } };
        object[] order = [new { title = "ASC" }];

        // Act
        using var publishedOnlyResponse = await SendProjectsAsync(
            client,
            includeUnpublished: false,
            where: where,
            order: order);

        using var allProjectsResponse = await SendProjectsAsync(
            client,
            includeUnpublished: true,
            where: where,
            order: order);

        // Assert
        var publishedOnlyData = await publishedOnlyResponse.ReadGraphQlDataAsync();
        var publishedOnly = publishedOnlyData.GetProperty("projects");
        var publishedItem = Assert.Single(
            publishedOnly.GetProperty("items").EnumerateArray());

        Assert.Equal(1, publishedOnly.GetProperty("totalCount").GetInt32());
        Assert.Equal(published.Id, publishedItem.GetProperty("id").GetGuid());

        var allProjectsData = await allProjectsResponse.ReadGraphQlDataAsync();
        var allProjects = allProjectsData.GetProperty("projects");
        var returnedIds = allProjects
            .GetProperty("items")
            .EnumerateArray()
            .Select(project => project.GetProperty("id").GetGuid())
            .ToArray();

        Assert.Equal(3, allProjects.GetProperty("totalCount").GetInt32());
        Assert.Equal([archived.Id, draft.Id, published.Id], returnedIds);
    }

    [Fact]
    public async Task Projects_AppliesFilteringSortingAndPaging()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var prefix = $"Admin paged {suffix}";
        var projectA = Project.Create($"{prefix} A", null, null);
        var projectB = Project.Create($"{prefix} B", null, null);
        var projectC = Project.Create($"{prefix} C", null, null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectA, projectB, projectC);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendProjectsAsync(
            client,
            includeUnpublished: true,
            skip: 1,
            take: 1,
            where: new { title = new { startsWith = prefix } },
            order: [new { title = "DESC" }]);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var projects = data.GetProperty("projects");
        var returnedProject = Assert.Single(projects.GetProperty("items").EnumerateArray());
        var pageInfo = projects.GetProperty("pageInfo");

        Assert.Equal(3, projects.GetProperty("totalCount").GetInt32());
        Assert.Equal(projectB.Id, returnedProject.GetProperty("id").GetGuid());
        Assert.True(pageInfo.GetProperty("hasNextPage").GetBoolean());
        Assert.True(pageInfo.GetProperty("hasPreviousPage").GetBoolean());
    }
}
