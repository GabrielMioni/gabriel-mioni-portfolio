using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
using Portfolio.Api.Domain.Projects;
using Portfolio.Api.GraphQL.Projects.Inputs;
using Portfolio.Api.GraphQL.Projects.Types;
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

    private static readonly HashSet<string> FullContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private static string ExtFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(contentType), $"Unsupported content type: {contentType}")
    };

    public async Task<IReadOnlyList<ProjectImageUploadInstruction>> RequestUploadsAsync(
        Guid projectId,
        IReadOnlyList<ProjectImageUploadRequestItemInput> items,
        CancellationToken ct)
    {
        if (items is null || items.Count == 0)
            throw new ArgumentException("No items provided", nameof(items));

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        // Validate individual items
        foreach (var item in items)
            ValidateItem(item);

        // Prevent duplicate variant requests
        var dup = items
            .GroupBy(i => new { i.ImageClientId, i.Variant })
            .FirstOrDefault(g => g.Count() > 1);

        if (dup != null)
        {
            throw new ArgumentException(
                $"Duplicate upload request for ImageClientId='{dup.Key.ImageClientId}', Variant='{dup.Key.Variant}'");
        }

        var groups = items
            .GroupBy(i => i.ImageClientId, StringComparer.Ordinal)
            .ToList();

        // Validate logical image groups
        foreach (var group in groups)
            ValidateImageGroup(group);

        var instructions = new List<ProjectImageUploadInstruction>(items.Count);

        var nextSortOrder = project.Images.Count == 0
            ? 0
            : project.Images.Max(x => x.SortOrder) + 1;

        foreach (var group in groups)
        {
            var full = group.Single(i => i.Variant == ProjectImageUploadVariant.Full);
            var thumb = group.Single(i => i.Variant == ProjectImageUploadVariant.Thumb);

            var imageGuid = Guid.NewGuid();

            var fullKey = $"projects/{projectId}/{imageGuid:N}_full.{ExtFor(full.ContentType)}";
            var thumbKey = $"projects/{projectId}/{imageGuid:N}_thumb.webp";

            var projectImage = ProjectImage.CreatePending(
                projectId,
                fullKey,
                thumbKey,
                nextSortOrder++
            );

            db.Set<ProjectImage>().Add(projectImage);

            instructions.Add(new ProjectImageUploadInstruction(
                projectId,
                group.Key,
                ProjectImageUploadVariant.Full,
                fullKey,
                _storage.CreatePresignedPutUrl(fullKey, full.ContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(fullKey)
            ));

            instructions.Add(new ProjectImageUploadInstruction(
                projectId,
                group.Key,
                ProjectImageUploadVariant.Thumb,
                thumbKey,
                _storage.CreatePresignedPutUrl(thumbKey, thumb.ContentType, TimeSpan.FromMinutes(5)),
                _storage.GetPublicUrl(thumbKey)
            ));
        }

        await db.SaveChangesAsync(ct);

        return instructions;
    }

    private static void ValidateItem(ProjectImageUploadRequestItemInput item)
    {
        if (string.IsNullOrWhiteSpace(item.ImageClientId))
            throw new ArgumentException("ImageClientId is required", nameof(item));

        if (string.IsNullOrWhiteSpace(item.ContentType))
            throw new ArgumentException("ContentType is required", nameof(item));

        if (item.SizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(item.SizeBytes), "SizeBytes must be > 0");

        switch (item.Variant)
        {
            case ProjectImageUploadVariant.Full:
                if (!FullContentTypes.Contains(item.ContentType))
                    throw new ArgumentException($"Invalid content type for FULL variant: {item.ContentType}");

                if (item.SizeBytes > 10 * 1024 * 1024)
                    throw new ArgumentException($"File size exceeds limit for FULL variant: {item.SizeBytes} bytes");
                break;

            case ProjectImageUploadVariant.Thumb:
                if (!string.Equals(item.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Invalid content type for THUMB variant: {item.ContentType}");

                if (item.SizeBytes > 600 * 1024)
                    throw new ArgumentException($"File size exceeds limit for THUMB variant: {item.SizeBytes} bytes");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(item.Variant), $"Unknown variant: {item.Variant}");
        }
    }

    private static void ValidateImageGroup(
        IGrouping<string, ProjectImageUploadRequestItemInput> group)
    {
        var fullCount = group.Count(i => i.Variant == ProjectImageUploadVariant.Full);
        var thumbCount = group.Count(i => i.Variant == ProjectImageUploadVariant.Thumb);

        if (fullCount != 1)
        {
            throw new ArgumentException(
                $"ImageClientId='{group.Key}' must contain exactly one FULL variant. Found {fullCount}.");
        }

        if (thumbCount != 1)
        {
            throw new ArgumentException(
                $"ImageClientId='{group.Key}' must contain exactly one THUMB variant. Found {thumbCount}.");
        }
    }
}