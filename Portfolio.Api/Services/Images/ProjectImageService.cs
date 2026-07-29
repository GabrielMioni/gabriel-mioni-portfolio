using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Admin.Inputs;
using Portfolio.Api.GraphQL.Projects.Admin.Payloads;
using Portfolio.Api.Services.Images.Helpers;
using Portfolio.Api.Services.Images.Results;
using Portfolio.Api.Services.Storage;

namespace Portfolio.Api.Services.Images;

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

    public async Task<PrepareProjectImageUploadsResult> PrepareImageUploadAsync(
        PrepareProjectImageUploadsInput input,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return PrepareProjectImageUploadsResult.NotFound();

        var imagesByClientId = project.Images
            .Where(image => image.ClientId is not null)
            .ToDictionary(image => image.ClientId!);

        var newImageCount = input.Items.Count(item =>
            !imagesByClientId.ContainsKey(item.ClientId.Trim()));

        if (project.Images.Count + newImageCount > Project.MaxImageCount)
            return PrepareProjectImageUploadsResult.ImageLimitExceeded();

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

            var projectImageId = Guid.NewGuid();

            var fullKey = $"projects/{projectId}/{projectImageId:N}_full.{ExtFor(item.FullContentType)}";
            var thumbKey = $"projects/{projectId}/{projectImageId:N}_thumb.{ExtFor(item.ThumbContentType)}";

            var projectImage = ProjectImage.CreatePending(
                id: projectImageId,
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

        return PrepareProjectImageUploadsResult.Success(instructions);
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

    public async Task<FinalizeProjectImageUploadsResult> FinalizeImageUploadAsync(
        FinalizeProjectImageUploadsInput input,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project is null)
            return FinalizeProjectImageUploadsResult.NotFound();

        var knownImageIds = project.Images
            .Select(image => image.Id)
            .ToHashSet();

        var invalidReferences = input.ProjectImageIds
            .Select((id, index) => new InvalidProjectImageReference(index, id))
            .Where(reference => !knownImageIds.Contains(reference.Id))
            .ToArray();

        if (invalidReferences.Length > 0)
            return FinalizeProjectImageUploadsResult.InvalidReference(invalidReferences);

        var requestedImages = input.ProjectImageIds
            .Select((id, index) => new { Id = id, InputIndex = index })
            .DistinctBy(reference => reference.Id)
            .Select(reference => new
            {
                reference.InputIndex,
                Image = project.Images.Single(image => image.Id == reference.Id)
            })
            .Where(request => !request.Image.IsUploaded)
            .ToArray();

        var uploadChecks = requestedImages.Select(async request =>
        {
            var objectExists = await Task.WhenAll(
                _storage.ObjectExistsAsync(request.Image.FullKey, ct),
                _storage.ObjectExistsAsync(request.Image.ThumbKey, ct));

            return new IncompleteProjectImageUpload(
                request.InputIndex,
                request.Image.Id,
                FullImageWasMissing: !objectExists[0],
                ThumbnailWasMissing: !objectExists[1]);
        });

        var incompleteUploads = (await Task.WhenAll(uploadChecks))
            .Where(upload => upload.FullImageWasMissing || upload.ThumbnailWasMissing)
            .ToArray();

        if (incompleteUploads.Length > 0)
            return FinalizeProjectImageUploadsResult.IncompleteUpload(incompleteUploads);

        foreach (var request in requestedImages)
            request.Image.MarkUploaded();

        if (requestedImages.Length > 0)
            await db.SaveChangesAsync(ct);

        return FinalizeProjectImageUploadsResult.Success(project);
    }

    public async Task<DeleteProjectImagesResult> DeleteProjectImagesAsync(
        DeleteProjectImagesInput input,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == input.ProjectId, ct);

        if (project is null)
            return DeleteProjectImagesResult.NotFound();

        var knownImageIds = project.Images
            .Select(image => image.Id)
            .ToHashSet();

        var invalidReferences = input.ProjectImageIds
            .Select((id, index) => new InvalidProjectImageReference(index, id))
            .Where(reference => !knownImageIds.Contains(reference.Id))
            .ToArray();

        if (invalidReferences.Length > 0)
            return DeleteProjectImagesResult.InvalidReference(invalidReferences);

        var targetIds = input.ProjectImageIds.ToHashSet();

        var imagesToDelete = project.Images
            .Where(i => targetIds.Contains(i.Id))
            .ToList();

        var deleteKeys = ProjectImageStorageKeyHelper.GetStorageKeys(imagesToDelete);

        await _storage.DeleteImagesAsync(deleteKeys, ct);

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

        if (imagesToDelete.Count > 0)
            await db.SaveChangesAsync(ct);

        return DeleteProjectImagesResult.Success(project);
    }
}
