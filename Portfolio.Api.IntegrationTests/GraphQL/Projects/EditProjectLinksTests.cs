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
public sealed class EditProjectLinksTests(SqlServerFixture database)
{
    private const string EditProjectLinksMutation =
        """
        mutation EditProjectLinks($input: EditProjectInput!) {
          editProject(input: $input) {
            project {
              id
              title
              links {
                id
                url
                linkText
                linkType
                sortOrder
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

    private sealed record LinkInput(
        Guid? Id,
        string Url,
        string LinkText,
        string LinkType,
        int SortOrder);

    private static Task<HttpResponseMessage> SendEditProjectLinksAsync(
        HttpClient client,
        Guid projectId,
        IReadOnlyList<LinkInput> links,
        IReadOnlyList<Guid>? removedLinkIds = null,
        string? title = null)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = EditProjectLinksMutation,
                variables = new
                {
                    input = new
                    {
                        id = projectId,
                        title,
                        links,
                        removedLinkIds
                    }
                }
            });
    }

    [Fact]
    public async Task EditProjectLinks_WithValidChanges_PersistsDesiredLinkSet()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            title: $"Project with links {Guid.NewGuid()}",
            summary: null,
            body: null);

        var linkToUpdate = ProjectLink.Create(
            projectId: project.Id,
            url: "https://example.com/old-repository",
            linkText: "Old repository",
            linkType: ProjectLinkType.External,
            sortOrder: 1);

        var linkToRemove = ProjectLink.Create(
            projectId: project.Id,
            url: "https://example.com/old-demo",
            linkText: "Old demo",
            linkType: ProjectLinkType.Demo,
            sortOrder: 0);

        project.AddLink(linkToUpdate);
        project.AddLink(linkToRemove);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        const string updatedUrl = "https://github.com/example/updated-repository";
        const string updatedText = "Updated repository";
        const string newUrlInput = "example.com/new-demo";
        const string normalizedNewUrl = "https://example.com/new-demo";
        const string newLinkText = "New demo";

        var links = new[]
        {
            new LinkInput(
                Id: linkToUpdate.Id,
                Url: updatedUrl,
                LinkText: updatedText,
                LinkType: "REPOSITORY",
                SortOrder: 10),
            new LinkInput(
                Id: null,
                Url: newUrlInput,
                LinkText: newLinkText,
                LinkType: "DEMO",
                SortOrder: -5)
        };

        // Act
        using var response = await SendEditProjectLinksAsync(
            client,
            projectId: project.Id,
            links,
            removedLinkIds: [linkToRemove.Id]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("editProject");
        var payloadProject = payload.GetProperty("project");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());

        var returnedLinks = payloadProject
            .GetProperty("links")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, returnedLinks.Length);

        var returnedUpdatedLink = Assert.Single(
            returnedLinks,
            link => link.GetProperty("id").GetGuid() == linkToUpdate.Id);

        Assert.Equal(updatedUrl, returnedUpdatedLink.GetProperty("url").GetString());
        Assert.Equal(updatedText, returnedUpdatedLink.GetProperty("linkText").GetString());
        Assert.Equal("REPOSITORY", returnedUpdatedLink.GetProperty("linkType").GetString());
        Assert.Equal(1, returnedUpdatedLink.GetProperty("sortOrder").GetInt32());

        var returnedNewLink = Assert.Single(
            returnedLinks,
            link => link.GetProperty("id").GetGuid() != linkToUpdate.Id);

        Assert.Equal(normalizedNewUrl, returnedNewLink.GetProperty("url").GetString());
        Assert.Equal(newLinkText, returnedNewLink.GetProperty("linkText").GetString());
        Assert.Equal("DEMO", returnedNewLink.GetProperty("linkType").GetString());
        Assert.Equal(0, returnedNewLink.GetProperty("sortOrder").GetInt32());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(p => p.Links)
            .AsNoTracking()
            .SingleAsync(p => p.Id == project.Id);

        Assert.Equal(2, persistedProject.Links.Count);
        Assert.DoesNotContain(
            persistedProject.Links,
            link => link.Id == linkToRemove.Id);

        var persistedUpdatedLink = Assert.Single(
            persistedProject.Links,
            link => link.Id == linkToUpdate.Id);

        Assert.Equal(updatedUrl, persistedUpdatedLink.Url);
        Assert.Equal(updatedText, persistedUpdatedLink.LinkText);
        Assert.Equal(ProjectLinkType.Repository, persistedUpdatedLink.LinkType);
        Assert.Equal(1, persistedUpdatedLink.SortOrder);

        var persistedNewLink = Assert.Single(
            persistedProject.Links,
            link => link.Id != linkToUpdate.Id);

        Assert.Equal(normalizedNewUrl, persistedNewLink.Url);
        Assert.Equal(newLinkText, persistedNewLink.LinkText);
        Assert.Equal(ProjectLinkType.Demo, persistedNewLink.LinkType);
        Assert.Equal(0, persistedNewLink.SortOrder);
    }

    [Fact]
    public async Task EditProjectLinks_WithForeignLinkId_ReturnsInvalidReferenceAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var originalTitle = $"Project with unchanged link {Guid.NewGuid()}";
        var project = Project.Create(
            title: originalTitle,
            summary: null,
            body: null);

        var existingLink = ProjectLink.Create(
            projectId: project.Id,
            url: "https://example.com/original",
            linkText: "Original link",
            linkType: ProjectLinkType.External,
            sortOrder: 0);

        project.AddLink(existingLink);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var foreignLinkId = Guid.NewGuid();
        var updatedTitle = $"Title that should not persist {Guid.NewGuid()}";
        var links = new[]
        {
            new LinkInput(
                Id: existingLink.Id,
                Url: "https://example.com/changed",
                LinkText: "Changed link",
                LinkType: "REPOSITORY",
                SortOrder: 1),
            new LinkInput(
                Id: foreignLinkId,
                Url: "https://example.com/foreign",
                LinkText: "Foreign link",
                LinkType: "DEMO",
                SortOrder: 0)
        };

        // Act
        using var response = await SendEditProjectLinksAsync(
            client,
            projectId: project.Id,
            links,
            title: updatedTitle);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("editProject");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project link '{foreignLinkId}' does not belong to project '{project.Id}'.",
            field: ["input", "links", "1", "id"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedProject = await verificationDb.Projects
            .Include(p => p.Links)
            .AsNoTracking()
            .SingleAsync(p => p.Id == project.Id);

        Assert.Equal(originalTitle, persistedProject.Title);

        var persistedLink = Assert.Single(persistedProject.Links);
        Assert.Equal(existingLink.Id, persistedLink.Id);
        Assert.Equal("https://example.com/original", persistedLink.Url);
        Assert.Equal("Original link", persistedLink.LinkText);
        Assert.Equal(ProjectLinkType.External, persistedLink.LinkType);
        Assert.Equal(0, persistedLink.SortOrder);
    }
}
