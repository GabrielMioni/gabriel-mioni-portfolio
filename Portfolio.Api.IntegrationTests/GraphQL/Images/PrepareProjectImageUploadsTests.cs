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
    public async Task PrepareProjectImageUploads_AtImageLimit_AllowsRetryButRejectsNewImage()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var suffix = TestData.NewSuffix();
        var project = Project.Create($"Project at image limit {suffix}", null, null);

        for (var index = 0; index < Project.MaxImageCount; index++)
        {
            project.AddImage(ProjectImage.CreatePending(
                id: Guid.NewGuid(),
                project.Id,
                clientId: $"existing-{index}-{suffix}",
                altText: $"Existing image {index}",
                fullKey: $"projects/{project.Id}/existing-{index}-full.webp",
                thumbKey: $"projects/{project.Id}/existing-{index}-thumb.webp",
                contentType: "image/webp",
                sizeBytes: 100,
                width: 100,
                height: 100,
                sortOrder: index));
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var retryItem = new PrepareItem(
            ClientId: $"existing-0-{suffix}",
            AltText: "Existing image 0",
            FullContentType: "image/webp",
            FullSizeBytes: 100,
            ThumbContentType: "image/webp",
            ThumbSizeBytes: 50,
            Height: 100,
            Width: 100);

        var newClientId = $"new-{suffix}";
        var newItem = retryItem with { ClientId = newClientId };

        // Act: retry an existing preparation at the limit
        using var retryResponse = await SendPrepareProjectImageUploadsAsync(
            client,
            project.Id,
            [retryItem]);

        // Assert: retry remains safe
        var retryPayload = await retryResponse.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");

        Assert.Empty(retryPayload.GetProperty("userErrors").EnumerateArray());
        Assert.Single(retryPayload.GetProperty("items").EnumerateArray());

        // Act: attempt to add a new image beyond the limit
        using var newImageResponse = await SendPrepareProjectImageUploadsAsync(
            client,
            project.Id,
            [newItem]);

        // Assert: public GraphQL contract
        var newImagePayload = await newImageResponse.ReadGraphQlPayloadAsync(
            "prepareProjectImageUploads");

        Assert.Equal(
            JsonValueKind.Null,
            newImagePayload.GetProperty("items").ValueKind);

        newImagePayload.AssertSingleUserError(
            code: GraphQlUserErrorCodes.Validation,
            message: $"A project cannot have more than {Project.MaxImageCount} images.",
            field: ["input", "items"]);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.Equal(
            Project.MaxImageCount,
            await verificationDb.ProjectImages.CountAsync(image =>
                image.ProjectId == project.Id));
        Assert.False(await verificationDb.ProjectImages.AnyAsync(image =>
            image.ClientId == newClientId));
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
    public async Task PrepareProjectImageUploads_WithInvalidItem_ReturnsValidationErrorsAndCreatesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            $"Project with invalid image {TestData.NewSuffix()}",
            null,
            null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var items = new[]
        {
            new PrepareItem(
                ClientId: " ",
                AltText: "Invalid image",
                FullContentType: "image/gif",
                FullSizeBytes: 0,
                ThumbContentType: "text/plain",
                ThumbSizeBytes: -1,
                Height: 0,
                Width: -1)
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

        var actualErrors = payload
            .GetProperty("userErrors")
            .EnumerateArray()
            .Select(error => (
                Code: error.GetProperty("code").GetString()!,
                Message: error.GetProperty("message").GetString()!,
                Field: string.Join(
                    ".",
                    error.GetProperty("field")
                        .EnumerateArray()
                        .Select(item => item.GetString()!))))
            .ToArray();

        var expectedErrors = new[]
        {
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Client ID is required.",
                Field: "input.items.0.clientId"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Content type must be image/jpeg, image/png, or image/webp.",
                Field: "input.items.0.fullContentType"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Content type must be image/jpeg, image/png, or image/webp.",
                Field: "input.items.0.thumbContentType"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Value must be greater than zero.",
                Field: "input.items.0.fullSizeBytes"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Value must be greater than zero.",
                Field: "input.items.0.thumbSizeBytes"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Value must be greater than zero.",
                Field: "input.items.0.width"),
            (
                Code: GraphQlUserErrorCodes.Validation,
                Message: "Value must be greater than zero.",
                Field: "input.items.0.height")
        };

        Assert.Equal(expectedErrors, actualErrors);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.ProjectImages
            .AnyAsync(image => image.ProjectId == project.Id));
    }

    [Fact]
    public async Task PrepareProjectImageUploads_AboveUploadOrTextLimits_ReturnsValidationErrorsAndCreatesNothing()
    {
        await using var factory = new ApiWebApplicationFactory(database.ConnectionString);
        using var client = factory.CreateAuthenticatedClient();

        // Arrange
        var project = Project.Create(
            $"Project with oversized image {TestData.NewSuffix()}",
            null,
            null);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
        }

        var items = new[]
        {
            new PrepareItem(
                ClientId: "oversized-image",
                AltText: new string('a', ProjectImage.MaxAltTextLength + 1),
                FullContentType: "image/jpeg",
                FullSizeBytes: ProjectImage.MaxFullSizeBytes + 1,
                ThumbContentType: "image/webp",
                ThumbSizeBytes: ProjectImage.MaxThumbnailSizeBytes + 1,
                Height: ProjectImage.MaxDimensionPixels + 1,
                Width: ProjectImage.MaxDimensionPixels + 1)
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

        var actualErrors = payload
            .GetProperty("userErrors")
            .EnumerateArray()
            .Select(error => (
                Message: error.GetProperty("message").GetString()!,
                Field: string.Join(
                    ".",
                    error.GetProperty("field")
                        .EnumerateArray()
                        .Select(item => item.GetString()!))))
            .ToArray();

        Assert.Equal(
            [
                (
                    Message: $"Alt text cannot exceed {ProjectImage.MaxAltTextLength} characters.",
                    Field: "input.items.0.altText"),
                (
                    Message: "Full-size image cannot exceed 15 MiB.",
                    Field: "input.items.0.fullSizeBytes"),
                (
                    Message: "Thumbnail cannot exceed 3 MiB.",
                    Field: "input.items.0.thumbSizeBytes"),
                (
                    Message: $"Image width cannot exceed {ProjectImage.MaxDimensionPixels} pixels.",
                    Field: "input.items.0.width"),
                (
                    Message: $"Image height cannot exceed {ProjectImage.MaxDimensionPixels} pixels.",
                    Field: "input.items.0.height")
            ],
            actualErrors);

        // Assert: persisted state
        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.False(await verificationDb.ProjectImages
            .AnyAsync(image => image.ProjectId == project.Id));
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
