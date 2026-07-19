using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services.Helpers;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.Services;

public class ProjectImageService
{
    private readonly IObjectStorage _storage;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProjectImageService(IObjectStorage storage, IDbContextFactory<AppDbContext> dbFactory)
    {
        _storage = storage;
        _dbFactory = dbFactory;
    }

    private static string ExtFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(contentType), $"Unsupported content type: {contentType}")
    };

    public async Task<IReadOnlyList<ProjectImageUploadInstruction>> PrepareImageUploadAsync(
        PrepareProjectImageUploadsInput input,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await GetProjectAsync(db, projectId, ct);

        var requestedClientIds = input.Items
            .Select(item => item.ClientId.Trim())
            .ToArray();

        var duplicateClientId = requestedClientIds
            .GroupBy(clientId => clientId)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateClientId is not null)
            throw new ArgumentException(
                $"Client ID '{duplicateClientId}' appears more than once.",
                nameof(input));

        var imagesByClientId = project.Images
            .Where(image => image.ClientId is not null)
            .ToDictionary(image => image.ClientId!);

        var instructions = new List<ProjectImageUploadInstruction>(input.Items.Count);

        var nextSortOrder = project.Images.Count == 0
            ? 0
            : project.Images.Max(x => x.SortOrder) + 1;

        foreach (var item in input.Items)
        {
            var clientId = item.ClientId.Trim();

            if (imagesByClientId.TryGetValue(clientId, out var existingImage))
            {
                instructions.Add(CreateUploadInstruction(existingImage, item));
                continue;
            }

            var imageId = Guid.NewGuid();

            var fullKey = $"projects/{projectId}/{imageId:N}_full.{ExtFor(item.FullContentType)}";
            var thumbKey = $"projects/{projectId}/{imageId:N}_thumb.{ExtFor(item.ThumbContentType)}";

            var projectImage = ProjectImage.CreatePending(
                projectId: projectId,
                clientId: clientId,
                altText: item.AltText,
                fullKey: fullKey,
                thumbKey: thumbKey,
                contentType: item.FullContentType,
                sizeBytes: item.FullSizeBytes,
                width: item.Width,
                height: item.Height,
                sortOrder: nextSortOrder++
            );

            project.AddImage(projectImage);
            imagesByClientId.Add(clientId, projectImage);
            instructions.Add(CreateUploadInstruction(projectImage, item));
        }

        await db.SaveChangesAsync(ct);

        return instructions;
    }

    private ProjectImageUploadInstruction CreateUploadInstruction(
        ProjectImage image,
        ProjectImagePrepareItem item)
    {
        var fullTarget = new ProjectImageUploadTarget(
            image.FullKey,
            _storage.CreatePresignedPutUrl(
                image.FullKey,
                item.FullContentType,
                TimeSpan.FromMinutes(5)),
            _storage.GetPublicUrl(image.FullKey),
            item.FullContentType
        );

        var thumbTarget = new ProjectImageUploadTarget(
            image.ThumbKey,
            _storage.CreatePresignedPutUrl(
                image.ThumbKey,
                item.ThumbContentType,
                TimeSpan.FromMinutes(5)),
            _storage.GetPublicUrl(image.ThumbKey),
            item.ThumbContentType
        );

        return new ProjectImageUploadInstruction(
            item.ClientId,
            image.Id,
            fullTarget,
            thumbTarget
        );
    }

    public async Task<Project> FinalizeImageUploadAsync(
        FinalizeProjectImageUploadsInput input,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await GetProjectAsync(db, projectId, ct);

        var targetIds = input.ProjectImageIds.ToHashSet();

        foreach (var image in project.Images)
        {
            if (targetIds.Contains(image.Id))
            {
                image.MarkUploaded();
            }
        }
        await db.SaveChangesAsync(ct);

        return project;
    }

    public async Task<Project> DeleteProjectImagesAsync(
    DeleteProjectImagesInput input,
    CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await GetProjectAsync(db, input.ProjectId, ct);

        var targetIds = input.ProjectImageIds.ToHashSet();

        var imagesToDelete = project.Images
            .Where(i => targetIds.Contains(i.Id))
            .ToList();

        var deleteKeys = ProjectImageStorageKeyHelper.GetStorageKeys(imagesToDelete);

        foreach (var image in imagesToDelete)
        {
            project.RemoveImage(image);
        }

        var orderedRemainingImages = project.Images
            .OrderBy(i => i.SortOrder)
            .ToList();

        for (var i = 0; i < orderedRemainingImages.Count; i++)
        {
            orderedRemainingImages[i].UpdateSortOrder(i);
        }

        await db.SaveChangesAsync(ct);

        await _storage.DeleteImagesAsync(deleteKeys, ct);

        return project;
    }

    private static async Task<Project> GetProjectAsync(
        AppDbContext db,
        Guid projectId,
        CancellationToken ct)
    {
        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        return project;
    }
}
