using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Payloads;
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

        var instructions = new List<ProjectImageUploadInstruction>(input.Items.Count);

        var nextSortOrder = project.Images.Count == 0
            ? 0
            : project.Images.Max(x => x.SortOrder) + 1;

        foreach (var item in input.Items)
        {
            var imageId = Guid.NewGuid();

            var fullKey = $"projects/{projectId}/{imageId:N}_full.{ExtFor(item.FullContentType)}";
            var thumbKey = $"projects/{projectId}/{imageId:N}_thumb.{ExtFor(item.ThumbContentType)}";

            var projectImage = ProjectImage.CreatePending(
                projectId,
                item.AltText,
                fullKey,
                thumbKey,
                item.FullContentType,
                item.FullSizeBytes,
                item.Width,
                item.Height,
                nextSortOrder++
            );

            db.Set<ProjectImage>().Add(projectImage);

            var fullTarget = new ProjectImageUploadTarget(
                fullKey,
                _storage.CreatePresignedPutUrl(fullKey, item.FullContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(fullKey),
                item.FullContentType
            );

            var thumbTarget = new ProjectImageUploadTarget(
                thumbKey,
                _storage.CreatePresignedPutUrl(thumbKey, item.ThumbContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(thumbKey),
                item.ThumbContentType
            );

            instructions.Add(new ProjectImageUploadInstruction(
                item.ClientId,
                projectImage.Id,
                fullTarget,
                thumbTarget
            ));
        }

        await db.SaveChangesAsync(ct);

        return instructions;
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

        var deleteKeys = new HashSet<string>();

        foreach (var image in imagesToDelete)
        {
            project.Images.Remove(image);
            deleteKeys.Add(image.FullKey);
            deleteKeys.Add(image.ThumbKey);
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