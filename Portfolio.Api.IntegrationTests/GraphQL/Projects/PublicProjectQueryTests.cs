using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicProjectQueryTests(SqlServerFixture database)
{
    private const string PublishedProjectsQuery =
        """
        query PublishedProjects(
          $skip: Int
          $take: Int
          $tagValues: [String!]
          $where: PublicProjectDtoFilterInput
          $order: [PublicProjectDtoSortInput!]
        ) {
          publishedProjects(
            skip: $skip
            take: $take
            tagValues: $tagValues
            where: $where
            order: $order
          ) {
            items {
              id
              title
              summary
              body
              publishedAt
              images {
                id
                fullKey
                thumbKey
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
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
            totalCount
          }
        }
        """;

    private const string PublishedTagsQuery =
        """
        query PublishedTags {
          publishedTags {
            id
            name
            value
          }
        }
        """;

    private static Task<HttpResponseMessage> SendPublishedProjectsAsync(
        HttpClient client,
        int? skip = null,
        int? take = null,
        string[]? tagValues = null,
        object? where = null,
        object[]? order = null)
    {
        return client.PostAsJsonAsync(
            "/graphql",
            new
            {
                query = PublishedProjectsQuery,
                variables = new
                {
                    skip,
                    take,
                    tagValues,
                    where,
                    order
                }
            });
    }

    private static Task<HttpResponseMessage> SendPublishedTagsAsync(HttpClient client)
    {
        return client.PostAsJsonAsync(
            "/graphql",
            new { query = PublishedTagsQuery });
    }

    [Fact]
    public async Task PublishedProjects_ReturnsOnlyPublishedProjectsWithOrderedNestedContent()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var published = Project.Create(
            title: $"Published query project {suffix}",
            summary: "Published summary",
            body: "Published body",
            status: ProjectStatus.Published);

        var draft = Project.Create(
            title: $"Draft query project {suffix}",
            summary: null,
            body: null);

        var archived = Project.Create(
            title: $"Archived query project {suffix}",
            summary: null,
            body: null,
            status: ProjectStatus.Archived);

        var secondImage = CreateUploadedImage(
            published.Id,
            suffix,
            sortOrder: 1,
            altText: "Second image");

        var firstImage = CreateUploadedImage(
            published.Id,
            suffix,
            sortOrder: 0,
            altText: "First image");

        published.AddImage(secondImage);
        published.AddImage(firstImage);

        var pendingImage = ProjectImage.CreatePending(
            id: Guid.NewGuid(),
            published.Id,
            clientId: $"pending-{suffix}",
            altText: "Pending image",
            fullKey: $"projects/{published.Id}/pending-full-{suffix}.webp",
            thumbKey: $"projects/{published.Id}/pending-thumb-{suffix}.webp",
            contentType: "image/webp",
            sizeBytes: 512,
            width: 600,
            height: 400,
            sortOrder: 2);
        published.AddImage(pendingImage);

        var secondLink = ProjectLink.Create(
            published.Id,
            "https://example.com/second",
            "Second link",
            ProjectLinkType.Demo,
            sortOrder: 1);

        var firstLink = ProjectLink.Create(
            published.Id,
            "https://example.com/first",
            "First link",
            ProjectLinkType.Repository,
            sortOrder: 0);

        published.AddLink(secondLink);
        published.AddLink(firstLink);

        var zuluTag = ProjectTag.Create($"Zulu {suffix}");
        var alphaTag = ProjectTag.Create($"Alpha {suffix}");
        published.AddTag(zuluTag);
        published.AddTag(alphaTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(published, draft, archived);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendPublishedProjectsAsync(
            client,
            where: new { title = new { contains = suffix } },
            order: [new { title = "ASC" }]);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var projects = data.GetProperty("publishedProjects");
        var returnedProject = Assert.Single(projects.GetProperty("items").EnumerateArray());

        Assert.Equal(1, projects.GetProperty("totalCount").GetInt32());
        Assert.Equal(published.Id, returnedProject.GetProperty("id").GetGuid());
        Assert.Equal(published.Title, returnedProject.GetProperty("title").GetString());
        Assert.Equal(published.Summary, returnedProject.GetProperty("summary").GetString());
        Assert.Equal(published.Body, returnedProject.GetProperty("body").GetString());
        Assert.NotEqual(
            System.Text.Json.JsonValueKind.Null,
            returnedProject.GetProperty("publishedAt").ValueKind);

        var returnedImages = returnedProject
            .GetProperty("images")
            .EnumerateArray()
            .ToArray();

        Assert.Equal([firstImage.Id, secondImage.Id], returnedImages.Select(image =>
            image.GetProperty("id").GetGuid()));
        Assert.Equal([0, 1], returnedImages.Select(image =>
            image.GetProperty("sortOrder").GetInt32()));
        Assert.Equal(
            [firstImage.FullKey, secondImage.FullKey],
            returnedImages.Select(image => image.GetProperty("fullKey").GetString()));
        Assert.Equal(
            [firstImage.ThumbKey, secondImage.ThumbKey],
            returnedImages.Select(image => image.GetProperty("thumbKey").GetString()));
        Assert.DoesNotContain(returnedImages, image =>
            image.GetProperty("id").GetGuid() == pendingImage.Id);

        var returnedLinks = returnedProject
            .GetProperty("links")
            .EnumerateArray()
            .ToArray();

        Assert.Equal([firstLink.Id, secondLink.Id], returnedLinks.Select(link =>
            link.GetProperty("id").GetGuid()));
        Assert.Equal(["REPOSITORY", "DEMO"], returnedLinks.Select(link =>
            link.GetProperty("linkType").GetString()));

        var returnedTags = returnedProject
            .GetProperty("tags")
            .EnumerateArray()
            .ToArray();

        Assert.Equal([alphaTag.Id, zuluTag.Id], returnedTags.Select(tag =>
            tag.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task PublishedProjects_WithTagValues_ReturnsPublishedProjectsWithAnyMatchingTag()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var selectedTag = ProjectTag.Create($"Selected {suffix}");
        var otherTag = ProjectTag.Create($"Other {suffix}");

        var selectedOnly = Project.Create(
            $"Selected only {suffix}",
            null,
            null,
            ProjectStatus.Published);
        selectedOnly.AddTag(selectedTag);

        var bothTags = Project.Create(
            $"Both tags {suffix}",
            null,
            null,
            ProjectStatus.Published);
        bothTags.AddTag(selectedTag);
        bothTags.AddTag(otherTag);

        var otherOnly = Project.Create(
            $"Other only {suffix}",
            null,
            null,
            ProjectStatus.Published);
        otherOnly.AddTag(otherTag);

        var selectedDraft = Project.Create(
            $"Selected draft {suffix}",
            null,
            null);
        selectedDraft.AddTag(selectedTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(selectedOnly, bothTags, otherOnly, selectedDraft);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendPublishedProjectsAsync(
            client,
            tagValues: [selectedTag.Value],
            where: new { title = new { contains = suffix } },
            order: [new { title = "ASC" }]);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var projects = data.GetProperty("publishedProjects");

        var returnedIds = projects
            .GetProperty("items")
            .EnumerateArray()
            .Select(project => project.GetProperty("id").GetGuid())
            .ToArray();

        Assert.Equal(2, projects.GetProperty("totalCount").GetInt32());
        Assert.Equal([bothTags.Id, selectedOnly.Id], returnedIds);
        Assert.DoesNotContain(otherOnly.Id, returnedIds);
        Assert.DoesNotContain(selectedDraft.Id, returnedIds);
    }

    [Fact]
    public async Task PublishedProjects_AppliesFilteringSortingAndPaging()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var prefix = $"Paged query {suffix}";

        var projectA = Project.Create($"{prefix} A", null, null, ProjectStatus.Published);
        var projectB = Project.Create($"{prefix} B", null, null, ProjectStatus.Published);
        var projectC = Project.Create($"{prefix} C", null, null, ProjectStatus.Published);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectC, projectA, projectB);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendPublishedProjectsAsync(
            client,
            skip: 1,
            take: 1,
            where: new { title = new { startsWith = prefix } },
            order: [new { title = "ASC" }]);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var projects = data.GetProperty("publishedProjects");
        var returnedProject = Assert.Single(projects.GetProperty("items").EnumerateArray());
        var pageInfo = projects.GetProperty("pageInfo");

        Assert.Equal(3, projects.GetProperty("totalCount").GetInt32());
        Assert.Equal(projectB.Id, returnedProject.GetProperty("id").GetGuid());
        Assert.True(pageInfo.GetProperty("hasNextPage").GetBoolean());
        Assert.True(pageInfo.GetProperty("hasPreviousPage").GetBoolean());
    }

    [Fact]
    public async Task PublishedTags_ReturnsOnlyTagsUsedByPublishedProjectsInNameOrder()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var zuluPublishedTag = ProjectTag.Create($"Zulu published {suffix}");
        var alphaPublishedTag = ProjectTag.Create($"Alpha published {suffix}");
        var draftTag = ProjectTag.Create($"Draft only {suffix}");
        var archivedTag = ProjectTag.Create($"Archived only {suffix}");
        var orphanedTag = ProjectTag.Create($"Orphaned {suffix}");

        var published = Project.Create(
            $"Published tag owner {suffix}",
            null,
            null,
            ProjectStatus.Published);
        published.AddTag(zuluPublishedTag);
        published.AddTag(alphaPublishedTag);

        var draft = Project.Create($"Draft tag owner {suffix}", null, null);
        draft.AddTag(draftTag);

        var archived = Project.Create(
            $"Archived tag owner {suffix}",
            null,
            null,
            ProjectStatus.Archived);
        archived.AddTag(archivedTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(published, draft, archived);
            db.Tags.Add(orphanedTag);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendPublishedTagsAsync(client);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var relevantTagIds = new[]
        {
            zuluPublishedTag.Id,
            alphaPublishedTag.Id,
            draftTag.Id,
            archivedTag.Id,
            orphanedTag.Id
        }.ToHashSet();

        var returnedTags = data
            .GetProperty("publishedTags")
            .EnumerateArray()
            .Where(tag => relevantTagIds.Contains(tag.GetProperty("id").GetGuid()))
            .ToArray();

        Assert.Equal(
            [alphaPublishedTag.Id, zuluPublishedTag.Id],
            returnedTags.Select(tag => tag.GetProperty("id").GetGuid()));
        Assert.DoesNotContain(returnedTags, tag =>
            tag.GetProperty("id").GetGuid() == draftTag.Id);
        Assert.DoesNotContain(returnedTags, tag =>
            tag.GetProperty("id").GetGuid() == archivedTag.Id);
        Assert.DoesNotContain(returnedTags, tag =>
            tag.GetProperty("id").GetGuid() == orphanedTag.Id);
    }

    private static ProjectImage CreateUploadedImage(
        Guid projectId,
        string suffix,
        int sortOrder,
        string altText)
    {
        var image = ProjectImage.CreatePending(
            id: Guid.NewGuid(),
            projectId,
            clientId: $"image-{sortOrder}-{suffix}",
            altText,
            fullKey: $"projects/{projectId}/full-{sortOrder}-{suffix}.webp",
            thumbKey: $"projects/{projectId}/thumb-{sortOrder}-{suffix}.webp",
            contentType: "image/webp",
            sizeBytes: 1_024,
            width: 1_200,
            height: 800,
            sortOrder);

        image.MarkUploaded();
        return image;
    }
}
