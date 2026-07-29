using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Tags;

[Collection(IntegrationTestCollection.Name)]
public sealed class AdminTagQueryTests(SqlServerFixture database)
{
    private const string TagsQuery =
        """
        query Tags {
          tags {
            id
            name
            value
          }
        }
        """;

    private const string ProjectsByTagIdQuery =
        """
        query ProjectsByTagId($tagId: UUID!) {
          projectsByTagId(tagId: $tagId) {
            id
            title
          }
        }
        """;

    private const string TagSummariesQuery =
        """
        query TagSummaries(
          $skip: Int
          $take: Int
          $showOrphaned: Boolean!
          $where: ProjectTagSummaryFilterInput
          $order: [ProjectTagSummarySortInput!]
        ) {
          tagSummaries(
            skip: $skip
            take: $take
            showOrphaned: $showOrphaned
            where: $where
            order: $order
          ) {
            items {
              id
              name
              value
              projectsCount
            }
            pageInfo {
              hasNextPage
              hasPreviousPage
            }
            totalCount
          }
        }
        """;

    private static Task<HttpResponseMessage> SendTagsAsync(HttpClient client)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new { query = TagsQuery });
    }

    private static Task<HttpResponseMessage> SendProjectsByTagIdAsync(
        HttpClient client,
        Guid tagId)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = ProjectsByTagIdQuery,
                variables = new { tagId }
            });
    }

    private static Task<HttpResponseMessage> SendTagSummariesAsync(
        HttpClient client,
        bool showOrphaned,
        int? skip = null,
        int? take = null,
        object? where = null,
        object[]? order = null)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = TagSummariesQuery,
                variables = new
                {
                    skip,
                    take,
                    showOrphaned,
                    where,
                    order
                }
            });
    }

    [Fact]
    public async Task Tags_ReturnsTagsInNameOrder()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var zuluTag = ProjectTag.Create($"Zulu admin tag {suffix}");
        var alphaTag = ProjectTag.Create($"Alpha admin tag {suffix}");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tags.AddRange(zuluTag, alphaTag);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendTagsAsync(client);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var expectedIds = new[] { alphaTag.Id, zuluTag.Id };
        var relevantTags = data
            .GetProperty("tags")
            .EnumerateArray()
            .Where(tag => expectedIds.Contains(tag.GetProperty("id").GetGuid()))
            .ToArray();

        Assert.Equal(
            expectedIds,
            relevantTags.Select(tag => tag.GetProperty("id").GetGuid()));
        Assert.Equal(
            [alphaTag.Value, zuluTag.Value],
            relevantTags.Select(tag => tag.GetProperty("value").GetString()));
    }

    [Fact]
    public async Task ProjectsByTagId_ReturnsOnlyAssignedProjectsInTitleOrder()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var tag = ProjectTag.Create($"Project lookup tag {suffix}");
        var projectB = Project.Create($"Tagged project B {suffix}", null, null);
        var projectA = Project.Create($"Tagged project A {suffix}", null, null);
        var untagged = Project.Create($"Untagged project {suffix}", null, null);

        projectB.AddTag(tag);
        projectA.AddTag(tag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(projectB, projectA, untagged);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendProjectsByTagIdAsync(client, tag.Id);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var returnedProjects = data
            .GetProperty("projectsByTagId")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(
            [projectA.Id, projectB.Id],
            returnedProjects.Select(project => project.GetProperty("id").GetGuid()));
        Assert.DoesNotContain(returnedProjects, project =>
            project.GetProperty("id").GetGuid() == untagged.Id);
    }

    [Fact]
    public async Task ProjectsByTagId_WhenTagDoesNotExist_ReturnsEmptyList()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Act
        using var response = await SendProjectsByTagIdAsync(client, Guid.NewGuid());

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        Assert.Empty(data.GetProperty("projectsByTagId").EnumerateArray());
    }

    [Fact]
    public async Task TagSummaries_ShowOrphanedControlsVisibilityAndReturnsProjectCounts()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var assignedTag = ProjectTag.Create($"Assigned summary {suffix}");
        var orphanedTag = ProjectTag.Create($"Orphaned summary {suffix}");
        var firstProject = Project.Create($"Summary project one {suffix}", null, null);
        var secondProject = Project.Create($"Summary project two {suffix}", null, null);

        firstProject.AddTag(assignedTag);
        secondProject.AddTag(assignedTag);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.AddRange(firstProject, secondProject);
            db.Tags.Add(orphanedTag);
            await db.SaveChangesAsync();
        }

        var where = new { name = new { contains = suffix } };
        object[] order = [new { name = "ASC" }];

        // Act
        using var withoutOrphansResponse = await SendTagSummariesAsync(
            client,
            showOrphaned: false,
            where: where,
            order: order);

        using var withOrphansResponse = await SendTagSummariesAsync(
            client,
            showOrphaned: true,
            where: where,
            order: order);

        // Assert
        var withoutOrphansData = await withoutOrphansResponse.ReadGraphQlDataAsync();
        var withoutOrphans = withoutOrphansData.GetProperty("tagSummaries");
        var assignedSummary = Assert.Single(
            withoutOrphans.GetProperty("items").EnumerateArray());

        Assert.Equal(1, withoutOrphans.GetProperty("totalCount").GetInt32());
        Assert.Equal(assignedTag.Id, assignedSummary.GetProperty("id").GetGuid());
        Assert.Equal(2, assignedSummary.GetProperty("projectsCount").GetInt32());

        var withOrphansData = await withOrphansResponse.ReadGraphQlDataAsync();
        var withOrphans = withOrphansData.GetProperty("tagSummaries");
        var summaries = withOrphans
            .GetProperty("items")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, withOrphans.GetProperty("totalCount").GetInt32());
        Assert.Equal(
            [assignedTag.Id, orphanedTag.Id],
            summaries.Select(summary => summary.GetProperty("id").GetGuid()));
        Assert.Equal(
            [2, 0],
            summaries.Select(summary => summary.GetProperty("projectsCount").GetInt32()));
    }

    [Fact]
    public async Task TagSummaries_AppliesFilteringSortingAndPaging()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var prefix = $"Paged summary {suffix}";
        var tagA = ProjectTag.Create($"{prefix} A");
        var tagB = ProjectTag.Create($"{prefix} B");
        var tagC = ProjectTag.Create($"{prefix} C");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Tags.AddRange(tagA, tagB, tagC);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendTagSummariesAsync(
            client,
            showOrphaned: true,
            skip: 1,
            take: 1,
            where: new { name = new { startsWith = prefix } },
            order: [new { name = "DESC" }]);

        // Assert
        var data = await response.ReadGraphQlDataAsync();
        var summaries = data.GetProperty("tagSummaries");
        var returnedSummary = Assert.Single(
            summaries.GetProperty("items").EnumerateArray());
        var pageInfo = summaries.GetProperty("pageInfo");

        Assert.Equal(3, summaries.GetProperty("totalCount").GetInt32());
        Assert.Equal(tagB.Id, returnedSummary.GetProperty("id").GetGuid());
        Assert.True(pageInfo.GetProperty("hasNextPage").GetBoolean());
        Assert.True(pageInfo.GetProperty("hasPreviousPage").GetBoolean());
    }
}
