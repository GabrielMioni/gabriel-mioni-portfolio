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
public sealed class FinalizeProjectImageUploadsTests(SqlServerFixture database)
{
    private const string FinalizeProjectImageUploadsMutation =
        """
        mutation FinalizeProjectImageUploads($input: FinalizeProjectImageUploadsInput!) {
          finalizeProjectImageUploads(input: $input) {
            project {
              id
              images {
                id
                fullKey
                thumbKey
                isUploaded
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

    private static Task<HttpResponseMessage> SendFinalizeProjectImageUploadsAsync(
        HttpClient client,
        Guid projectId,
        IReadOnlyList<Guid> projectImageIds)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = FinalizeProjectImageUploadsMutation,
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
    public async Task FinalizeProjectImageUploads_WhenObjectsExist_MarksImageUploaded()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with uploaded image {suffix}", null, null);
        var projectImageId = Guid.NewGuid();
        var fullKey = $"projects/{project.Id}/{projectImageId:N}_full.jpg";
        var thumbKey = $"projects/{project.Id}/{projectImageId:N}_thumb.webp";

        project.AddImage(ProjectImage.CreatePending(
            id: projectImageId,
            projectId: project.Id,
            clientId: $"uploaded-{suffix}",
            altText: "Uploaded image",
            fullKey: fullKey,
            thumbKey: thumbKey,
            contentType: "image/jpeg",
            sizeBytes: 120_000,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            projectImageIds: [projectImageId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync(
            "finalizeProjectImageUploads");

        Assert.Empty(payload.GetProperty("userErrors").EnumerateArray());

        var payloadProject = payload.GetProperty("project");
        Assert.Equal(project.Id, payloadProject.GetProperty("id").GetGuid());

        var payloadImage = Assert.Single(
            payloadProject.GetProperty("images").EnumerateArray());

        Assert.Equal(projectImageId, payloadImage.GetProperty("id").GetGuid());
        Assert.Equal(fullKey, payloadImage.GetProperty("fullKey").GetString());
        Assert.Equal(thumbKey, payloadImage.GetProperty("thumbKey").GetString());
        Assert.True(payloadImage.GetProperty("isUploaded").GetBoolean());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImage = await verificationDb.ProjectImages
            .AsNoTracking()
            .SingleAsync(image => image.Id == projectImageId);

        Assert.True(persistedImage.IsUploaded);
    }

    [Fact]
    public async Task FinalizeProjectImageUploads_WhenRetried_ReturnsAlreadyUploadedImageWithoutRecheckingStorage()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with retried image finalization {suffix}", null, null);
        var projectImageId = Guid.NewGuid();
        var fullKey = $"projects/{project.Id}/{projectImageId:N}_full.jpg";
        var thumbKey = $"projects/{project.Id}/{projectImageId:N}_thumb.webp";

        project.AddImage(ProjectImage.CreatePending(
            id: projectImageId,
            projectId: project.Id,
            clientId: $"retried-{suffix}",
            altText: "Retried image",
            fullKey: fullKey,
            thumbKey: thumbKey,
            contentType: "image/jpeg",
            sizeBytes: 120_000,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        using (var firstResponse = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            projectImageIds: [projectImageId]))
        {
            var firstPayload = await firstResponse.ReadGraphQlPayloadAsync(
                "finalizeProjectImageUploads");

            Assert.Empty(firstPayload.GetProperty("userErrors").EnumerateArray());
            Assert.True(firstPayload
                .GetProperty("project")
                .GetProperty("images")[0]
                .GetProperty("isUploaded")
                .GetBoolean());
        }

        factory.ObjectStorage.SetObjectMissing(fullKey);
        factory.ObjectStorage.SetObjectMissing(thumbKey);

        // Act
        using var retryResponse = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            projectImageIds: [projectImageId]);

        // Assert: public GraphQL contract
        var retryPayload = await retryResponse.ReadGraphQlPayloadAsync(
            "finalizeProjectImageUploads");

        Assert.Empty(retryPayload.GetProperty("userErrors").EnumerateArray());

        var retryProject = retryPayload.GetProperty("project");
        Assert.Equal(project.Id, retryProject.GetProperty("id").GetGuid());

        var retryImage = Assert.Single(
            retryProject.GetProperty("images").EnumerateArray());

        Assert.Equal(projectImageId, retryImage.GetProperty("id").GetGuid());
        Assert.True(retryImage.GetProperty("isUploaded").GetBoolean());

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImage = await verificationDb.ProjectImages
            .AsNoTracking()
            .SingleAsync(image => image.Id == projectImageId);

        Assert.True(persistedImage.IsUploaded);
    }

    [Fact]
    public async Task FinalizeProjectImageUploads_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();

        // Act
        using var response = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: missingProjectId,
            projectImageIds: []);

        // Assert
        var payload = await response.ReadGraphQlPayloadAsync(
            "finalizeProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.NotFound,
            message: $"Project '{missingProjectId}' was not found.",
            field: ["input", "projectId"]);
    }

    [Fact]
    public async Task FinalizeProjectImageUploads_WithForeignImageId_ReturnsInvalidReferenceAndChangesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with foreign image {suffix}", null, null);
        var projectImageId = Guid.NewGuid();
        var missingImageId = Guid.NewGuid();

        project.AddImage(ProjectImage.CreatePending(
            id: projectImageId,
            projectId: project.Id,
            clientId: $"pending-{suffix}",
            altText: "Pending image",
            fullKey: $"projects/{project.Id}/{projectImageId:N}_full.jpg",
            thumbKey: $"projects/{project.Id}/{projectImageId:N}_thumb.webp",
            contentType: "image/jpeg",
            sizeBytes: 120_000,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        // Act
        using var response = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            projectImageIds: [projectImageId, missingImageId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync(
            "finalizeProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidReference,
            message: $"Project image '{missingImageId}' was not found on this project.",
            field: ["input", "projectImageIds", "1"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImage = await verificationDb.ProjectImages
            .AsNoTracking()
            .SingleAsync(image => image.Id == projectImageId);

        Assert.False(persistedImage.IsUploaded);
    }

    [Theory]
    [InlineData(MissingStorageObject.FullImage)]
    [InlineData(MissingStorageObject.Thumbnail)]
    [InlineData(MissingStorageObject.FullImageAndThumbnail)]
    public async Task FinalizeProjectImageUploads_WhenStorageObjectIsMissing_ReturnsInvalidStateAndChangesNothing(
        MissingStorageObject missingObject)
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with incomplete image {suffix}", null, null);
        var projectImageId = Guid.NewGuid();
        var fullKey = $"projects/{project.Id}/{projectImageId:N}_full.jpg";
        var thumbKey = $"projects/{project.Id}/{projectImageId:N}_thumb.webp";

        project.AddImage(ProjectImage.CreatePending(
            id: projectImageId,
            projectId: project.Id,
            clientId: $"incomplete-{suffix}",
            altText: "Incomplete image",
            fullKey: fullKey,
            thumbKey: thumbKey,
            contentType: "image/jpeg",
            sizeBytes: 120_000,
            width: 1_200,
            height: 800,
            sortOrder: 0));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        switch (missingObject)
        {
            case MissingStorageObject.FullImage:
                factory.ObjectStorage.SetObjectMissing(fullKey);
                break;
            case MissingStorageObject.Thumbnail:
                factory.ObjectStorage.SetObjectMissing(thumbKey);
                break;
            case MissingStorageObject.FullImageAndThumbnail:
                factory.ObjectStorage.SetObjectMissing(fullKey);
                factory.ObjectStorage.SetObjectMissing(thumbKey);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(missingObject));
        }

        var expectedMessage = missingObject switch
        {
            MissingStorageObject.FullImage =>
                $"Project image '{projectImageId}' is missing its full-size image in storage.",
            MissingStorageObject.Thumbnail =>
                $"Project image '{projectImageId}' is missing its thumbnail in storage.",
            MissingStorageObject.FullImageAndThumbnail =>
                $"Project image '{projectImageId}' is missing its full-size image and thumbnail in storage.",
            _ => throw new ArgumentOutOfRangeException(nameof(missingObject))
        };

        // Act
        using var response = await SendFinalizeProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            projectImageIds: [projectImageId]);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync(
            "finalizeProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("project").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.InvalidState,
            message: expectedMessage,
            field: ["input", "projectImageIds", "0"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImage = await verificationDb.ProjectImages
            .AsNoTracking()
            .SingleAsync(image => image.Id == projectImageId);

        Assert.False(persistedImage.IsUploaded);
    }

    public enum MissingStorageObject
    {
        FullImage,
        Thumbnail,
        FullImageAndThumbnail
    }
}
