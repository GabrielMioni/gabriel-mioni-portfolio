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
public sealed class PrepareProjectImageUploadsTests(SqlServerFixture database)
{
    private const string PrepareProjectImageUploadsMutation =
        """
        mutation PrepareProjectImageUploads($input: PrepareProjectImageUploadsInput!) {
          prepareProjectImageUploads(input: $input) {
            items {
              clientId
              projectImageId
              full {
                key
                uploadUrl
                publicUrl
                contentType
              }
              thumb {
                key
                uploadUrl
                publicUrl
                contentType
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

    private sealed record PrepareItem(
        string ClientId,
        string AltText,
        string FullContentType,
        int FullSizeBytes,
        string ThumbContentType,
        int ThumbSizeBytes,
        int Height,
        int Width);

    private sealed record ReturnedTarget(
        string Key,
        string UploadUrl,
        string PublicUrl,
        string ContentType);

    private sealed record ReturnedInstruction(
        string ClientId,
        Guid ProjectImageId,
        ReturnedTarget Full,
        ReturnedTarget Thumb);

    private static Task<HttpResponseMessage> SendPrepareProjectImageUploadsAsync(
        HttpClient client,
        Guid projectId,
        IReadOnlyList<PrepareItem> items)
    {
        return client.PostAsJsonAsync(
            "/graphql/admin",
            new
            {
                query = PrepareProjectImageUploadsMutation,
                variables = new
                {
                    input = new
                    {
                        projectId,
                        items
                    }
                }
            });
    }

    private static IReadOnlyDictionary<string, ReturnedInstruction> ReadInstructions(
        JsonElement payload)
    {
        return payload
            .GetProperty("items")
            .EnumerateArray()
            .Select(item => new ReturnedInstruction(
                ClientId: item.GetProperty("clientId").GetString()!,
                ProjectImageId: item.GetProperty("projectImageId").GetGuid(),
                Full: ReadTarget(item.GetProperty("full")),
                Thumb: ReadTarget(item.GetProperty("thumb"))))
            .ToDictionary(item => item.ClientId);
    }

    private static ReturnedTarget ReadTarget(JsonElement target)
    {
        return new ReturnedTarget(
            Key: target.GetProperty("key").GetString()!,
            UploadUrl: target.GetProperty("uploadUrl").GetString()!,
            PublicUrl: target.GetProperty("publicUrl").GetString()!,
            ContentType: target.GetProperty("contentType").GetString()!);
    }

    private static void AssertInitialTarget(
        ReturnedTarget target,
        string expectedKey,
        string expectedContentType)
    {
        Assert.Equal(expectedKey, target.Key);
        Assert.Equal(expectedContentType, target.ContentType);
        Assert.Equal($"https://storage.test/upload/{expectedKey}", target.UploadUrl);
        Assert.Equal($"https://storage.test/{expectedKey}", target.PublicUrl);
    }

    [Fact]
    public async Task PrepareProjectImageUploads_WhenRetried_ReusesPendingImages()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with pending images {suffix}", null, null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var fullImageClientId = $"full-{suffix}";
        var thumbnailImageClientId = $"thumbnail-{suffix}";

        var items = new[]
        {
            new PrepareItem(
                ClientId: fullImageClientId,
                AltText: "Full image",
                FullContentType: "image/jpeg",
                FullSizeBytes: 120_000,
                ThumbContentType: "image/webp",
                ThumbSizeBytes: 12_000,
                Height: 800,
                Width: 1200),
            new PrepareItem(
                ClientId: thumbnailImageClientId,
                AltText: "Thumbnail image",
                FullContentType: "image/png",
                FullSizeBytes: 90_000,
                ThumbContentType: "image/jpeg",
                ThumbSizeBytes: 9_000,
                Height: 600,
                Width: 900)
        };

        // Act: initial request
        using var firstResponse = await SendPrepareProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            items);

        var firstPayload = await firstResponse.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");
        var firstInstructions = ReadInstructions(firstPayload);

        // Assert: initial public GraphQL contract
        Assert.Empty(firstPayload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(2, firstInstructions.Count);

        var firstFullImage = firstInstructions[fullImageClientId];
        var firstThumbnailImage = firstInstructions[thumbnailImageClientId];

        Assert.NotEqual(Guid.Empty, firstFullImage.ProjectImageId);
        Assert.NotEqual(Guid.Empty, firstThumbnailImage.ProjectImageId);
        Assert.NotEqual(
            firstFullImage.ProjectImageId,
            firstThumbnailImage.ProjectImageId);

        var expectedFullImageFullKey =
            $"projects/{project.Id}/{firstFullImage.ProjectImageId:N}_full.jpg";
        var expectedFullImageThumbKey =
            $"projects/{project.Id}/{firstFullImage.ProjectImageId:N}_thumb.webp";
        var expectedThumbnailImageFullKey =
            $"projects/{project.Id}/{firstThumbnailImage.ProjectImageId:N}_full.png";
        var expectedThumbnailImageThumbKey =
            $"projects/{project.Id}/{firstThumbnailImage.ProjectImageId:N}_thumb.jpg";

        AssertInitialTarget(
            firstFullImage.Full,
            expectedFullImageFullKey,
            expectedContentType: "image/jpeg");
        AssertInitialTarget(
            firstFullImage.Thumb,
            expectedFullImageThumbKey,
            expectedContentType: "image/webp");
        AssertInitialTarget(
            firstThumbnailImage.Full,
            expectedThumbnailImageFullKey,
            expectedContentType: "image/png");
        AssertInitialTarget(
            firstThumbnailImage.Thumb,
            expectedThumbnailImageThumbKey,
            expectedContentType: "image/jpeg");

        // Act: retry the same request
        using var retryResponse = await SendPrepareProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            items);

        var retryPayload = await retryResponse.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");
        var retryInstructions = ReadInstructions(retryPayload);

        // Assert: retry-safe public contract
        Assert.Empty(retryPayload.GetProperty("userErrors").EnumerateArray());
        Assert.Equal(2, retryInstructions.Count);

        Assert.Equal(
            firstFullImage.ProjectImageId,
            retryInstructions[fullImageClientId].ProjectImageId);
        Assert.Equal(
            firstFullImage.Full.Key,
            retryInstructions[fullImageClientId].Full.Key);
        Assert.Equal(
            firstFullImage.Thumb.Key,
            retryInstructions[fullImageClientId].Thumb.Key);

        Assert.Equal(
            firstThumbnailImage.ProjectImageId,
            retryInstructions[thumbnailImageClientId].ProjectImageId);
        Assert.Equal(
            firstThumbnailImage.Full.Key,
            retryInstructions[thumbnailImageClientId].Full.Key);
        Assert.Equal(
            firstThumbnailImage.Thumb.Key,
            retryInstructions[thumbnailImageClientId].Thumb.Key);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var persistedImages = await verificationDb.ProjectImages
            .AsNoTracking()
            .Where(image => image.ProjectId == project.Id)
            .ToDictionaryAsync(image => image.ClientId!);

        Assert.Equal(2, persistedImages.Count);

        var persistedFullImage = persistedImages[fullImageClientId];
        Assert.Equal(firstFullImage.ProjectImageId, persistedFullImage.Id);
        Assert.Equal("Full image", persistedFullImage.AltText);
        Assert.Equal(expectedFullImageFullKey, persistedFullImage.FullKey);
        Assert.Equal(expectedFullImageThumbKey, persistedFullImage.ThumbKey);
        Assert.Equal("image/jpeg", persistedFullImage.ContentType);
        Assert.Equal(120_000, persistedFullImage.SizeBytes);
        Assert.Equal(1200, persistedFullImage.Width);
        Assert.Equal(800, persistedFullImage.Height);
        Assert.Equal(0, persistedFullImage.SortOrder);
        Assert.False(persistedFullImage.IsUploaded);

        var persistedThumbnailImage = persistedImages[thumbnailImageClientId];
        Assert.Equal(firstThumbnailImage.ProjectImageId, persistedThumbnailImage.Id);
        Assert.Equal("Thumbnail image", persistedThumbnailImage.AltText);
        Assert.Equal(expectedThumbnailImageFullKey, persistedThumbnailImage.FullKey);
        Assert.Equal(expectedThumbnailImageThumbKey, persistedThumbnailImage.ThumbKey);
        Assert.Equal("image/png", persistedThumbnailImage.ContentType);
        Assert.Equal(90_000, persistedThumbnailImage.SizeBytes);
        Assert.Equal(900, persistedThumbnailImage.Width);
        Assert.Equal(600, persistedThumbnailImage.Height);
        Assert.Equal(1, persistedThumbnailImage.SortOrder);
        Assert.False(persistedThumbnailImage.IsUploaded);
    }

    [Fact]
    public async Task PrepareProjectImageUploads_WhenProjectDoesNotExist_ReturnsNotFoundAndCreatesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var missingProjectId = Guid.NewGuid();
        var clientId = $"missing-project-{TestData.NewSuffix()}";
        var items = new[]
        {
            new PrepareItem(
                ClientId: clientId,
                AltText: "Image for missing project",
                FullContentType: "image/jpeg",
                FullSizeBytes: 100,
                ThumbContentType: "image/webp",
                ThumbSizeBytes: 50,
                Height: 100,
                Width: 100)
        };

        // Act
        using var response = await SendPrepareProjectImageUploadsAsync(
            client,
            projectId: missingProjectId,
            items);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("items").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.NotFound,
            message: $"Project '{missingProjectId}' was not found.",
            field: ["input", "projectId"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.ProjectImages
            .AnyAsync(image => image.ClientId == clientId));
    }

    [Fact]
    public async Task PrepareProjectImageUploads_WithDuplicateClientIds_ReturnsConflictAndCreatesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project with rejected images {suffix}", null, null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var clientId = $"duplicate-{suffix}";
        var items = new[]
        {
            new PrepareItem(
                ClientId: $"  {clientId}  ",
                AltText: "First image",
                FullContentType: "image/jpeg",
                FullSizeBytes: 100,
                ThumbContentType: "image/webp",
                ThumbSizeBytes: 50,
                Height: 100,
                Width: 100),
            new PrepareItem(
                ClientId: clientId,
                AltText: "Second image",
                FullContentType: "image/png",
                FullSizeBytes: 100,
                ThumbContentType: "image/jpeg",
                ThumbSizeBytes: 50,
                Height: 100,
                Width: 100)
        };

        // Act
        using var response = await SendPrepareProjectImageUploadsAsync(
            client,
            projectId: project.Id,
            items);

        // Assert: public GraphQL contract
        var payload = await response.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            payload.GetProperty("items").ValueKind);

        payload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Conflict,
            message: $"Client ID '{clientId}' duplicates another requested image.",
            field: ["input", "items", "1", "clientId"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.ProjectImages
            .AnyAsync(image => image.ProjectId == project.Id));
    }
}
