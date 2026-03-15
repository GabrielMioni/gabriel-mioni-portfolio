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

    private static string ExtFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(contentType), $"Unsupported content type: {contentType}")
    };

    public async Task<IReadOnlyList<ProjectImageUploadInstruction>> PrepareImageUploadAsync(
        PrepareProjectImageUploadsInput input,
        CancellationToken ct
    ) {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var projectId = input.ProjectId;

        var project = await db.Projects
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == projectId, ct);

        if (project == null)
            throw new InvalidOperationException("Project not found");

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
}