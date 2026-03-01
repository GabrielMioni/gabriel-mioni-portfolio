using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Data;
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
    // If you ever want to allow it for full-size:
    // "image/webp"
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

        var projectExists = await db.Projects.AnyAsync(p => p.Id == projectId, ct);
        if (!projectExists)
            throw new InvalidOperationException("Project not found");

        var dup = items
          .GroupBy(i => new { i.ImageClientId, i.Variant })
          .FirstOrDefault(g => g.Count() > 1);

        if (dup != null)
            throw new ArgumentException(
              $"Duplicate upload request for ImageClientId='{dup.Key.ImageClientId}', Variant='{dup.Key.Variant}'");

        // 1 GUID per ImageClientId, shared across Full/Thumb variants
        var imageGuidsByClientId = items
          .Select(i => i.ImageClientId)
          .Distinct(StringComparer.Ordinal)
          .ToDictionary(id => id, _ => Guid.NewGuid(), StringComparer.Ordinal);

        var instructions = new List<ProjectImageUploadInstruction>(items.Count);

        foreach (var item in items)
        {
            ValidateItem(item);

            var imageGuid = imageGuidsByClientId[item.ImageClientId];

            var key = item.Variant switch
            {
                ProjectImageUploadVariant.Full =>
                  $"projects/{projectId}/{imageGuid:N}_full.{ExtFor(item.ContentType)}",

                ProjectImageUploadVariant.Thumb =>
                  $"projects/{projectId}/{imageGuid:N}_thumb.webp",

                _ => throw new ArgumentOutOfRangeException(nameof(item.Variant), $"Unknown variant: {item.Variant}")
            };

            var uploadUrl = _storage.CreatePresignedPutUrl(
              key,
              item.ContentType,
              TimeSpan.FromMinutes(5));

            var publicUrl = _storage.GetPublicUrl(key);

            instructions.Add(new ProjectImageUploadInstruction(
              item.ImageClientId,
              item.Variant,
              key,
              uploadUrl,
              publicUrl));
        }

        return instructions;
    }

    private static void ValidateItem(ProjectImageUploadRequestItemInput item)
    {
        if (string.IsNullOrWhiteSpace(item.ImageClientId))
            throw new ArgumentException("ImageClientId is required");

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
}