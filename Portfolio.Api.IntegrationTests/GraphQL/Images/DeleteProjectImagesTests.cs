using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace Portfolio.Api.IntegrationTests.GraphQL.Images;

[Collection(IntegrationTestCollection.Name)]
public sealed class DeleteProjectImagesTests(SqlServerFixture database)
{
    private const string DeleteProjectImagesMutation =
        """
        mutation DeleteProjectImages($input: DeleteProjectImagesInput!) {
          deleteProjectImages(input: $input) {
            project {
              id
              images {
                id
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

    private static Task<HttpResponseMessage> SendDeleteProjectImagesAsync(
        HttpClient client,
        Guid projectId,
        IReadOnlyList<Guid> projectImageIds)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = DeleteProjectImagesMutation,
                variables = new
                {
                    input = new
                    {
                        projectId,
                        projectImageIds
                    }
                }
            });
    }

    [Fact]
    public async Task DeleteProjectImages_WithExistingImage_DeletesStoredObjectsAndReordersRemainingImages()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with image to delete {suffix}", null, null);
        var firstImage = AddUploadedImage(project, $"first-{suffix}", sortOrder: 0);
        var deletedImage = AddUploadedImage(project, $"deleted-{suffix}", sortOrder: 1);
        var lastImage = AddUploadedImage(project, $"last-{suffix}", sortOrder: 2);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendDeleteProjectImagesAsync(
            client,
            projectId: project.Id,
            projectImageIds: [deletedImage.Id]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjectImages");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        var payloadProject = payload.GetProperty("project");
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());

        var payloadImages = payloadProject
            .GetProperty("images")
            .EnumerateArray()
            .OrderBy(image => image.GetProperty("sortOrder").GetInt32())
            .ToArray();

        Assert.Collection(
            payloadImages,
            image =>
            {
                Assert.Equal(firstImage.Id, image.GetProperty("id").GetGuid());
                Assert.Equal(0, image.GetProperty("sortOrder").GetInt32());
            },
            image =>
            {
                Assert.Equal(lastImage.Id, image.GetProperty("id").GetGuid());
                Assert.Equal(1, image.GetProperty("sortOrder").GetInt32());
            });

        Assert.Equal(
            new[] { deletedImage.FullKey, deletedImage.ThumbKey }.Order(),
            factory.ObjectStorage.DeletedKeys.Order());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.ProjectImages
            .AnyAsync(image => image.Id == deletedImage.Id));

        var persistedImages = await verificationDb.ProjectImages
            .AsNoTracking()
            .Where(image => image.ProjectId == project.Id)
            .OrderBy(image => image.SortOrder)
            .ToArrayAsync();

        Assert.Collection(
            persistedImages,
            image =>
            {
                Assert.Equal(firstImage.Id, image.Id);
                Assert.Equal(0, image.SortOrder);
            },
            image =>
            {
                Assert.Equal(lastImage.Id, image.Id);
                Assert.Equal(1, image.SortOrder);
            });
    }

    [Fact]
    public async Task DeleteProjectImages_WhenProjectDoesNotExist_ReturnsNotFoundAndDeletesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendDeleteProjectImagesAsync(
            client,
            projectId: missingProjectId,
            projectImageIds: [Guid.NewGuid()]);

        // Assert
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjectImages");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.NotFound,
            message: $"Project '{missingProjectId}' was not found.",
            field: ["input", "projectId"]);

        Assert.Empty(factory.ObjectStorage.DeletedKeys);
    }

    [Fact]
    public async Task DeleteProjectImages_WithForeignImageId_ReturnsInvalidReferenceAndDeletesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with retained image {suffix}", null, null);
        var retainedImage = AddUploadedImage(project, $"retained-{suffix}", sortOrder: 0);
        var missingImageId = Guid.NewGuid();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendDeleteProjectImagesAsync(
            client,
            projectId: project.Id,
            projectImageIds: [retainedImage.Id, missingImageId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync("deleteProjectImages");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project image '{missingImageId}' was not found on this project.",
            field: ["input", "projectImageIds", "1"]);

        Assert.Empty(factory.ObjectStorage.DeletedKeys);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImage = await verificationDb.ProjectImages
            .AsNoTracking()
            .SingleAsync(image => image.Id == retainedImage.Id);

        Assert.Equal(0, persistedImage.SortOrder);
    }

    private static ProjectImage AddUploadedImage(
        Project project,
        string clientId,
        int sortOrder)
    {
        var imageId = Guid.NewGuid();
        var image = ProjectImage.CreatePending(
            id: imageId,
            projectId: project.Id,
            clientId: clientId,
            altText: null,
            fullKey: $"projects/{project.Id}/{imageId:N}_full.jpg",
            thumbKey: $"projects/{project.Id}/{imageId:N}_thumb.webp",
            contentType: "image/jpeg",
            sizeBytes: 120_000,
            width: 1_200,
            height: 800,
            sortOrder: sortOrder);

        image.MarkUploaded();
        project.AddImage(image);

        return image;
    }
}
